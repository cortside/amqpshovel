using System;
using System.Collections.Generic;
using Amqp;
using CommandLine;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;

namespace AmqpTools.Core.Commands.Peek {
    public class DeleteMessageCommand : IServiceCommand<DeleteMessageOptions, bool> {
        private const int EXIT_SUCCESS = 0;
        const int ERROR_NO_MESSAGE = 1;
        const int ERROR_OTHER = 2;

        private ParserResult<DeleteMessageOptions> result;

        public ILogger Logger { get; set; }

        public void ParseArguments(string[] args) {
            result = Parser.Default.ParseArguments<DeleteMessageOptions>(args);
            result.WithParsed(opts => {
                opts.ApplyConfig();
            });

        }

        public int Execute() {
            result
            .WithParsed(opts => {
                DeleteMessage(opts);
            })
                .WithNotParsed(errors => {
                    foreach (var error in errors) {
                        Console.Out.WriteLine(error.ToString());
                    }
                });

            return EXIT_SUCCESS;
        }

        public bool ServiceExecute(DeleteMessageOptions options) {
            return DeleteMessage(options);
        }

        private bool DeleteMessage(DeleteMessageOptions opts) {
            var success = false;

            string formattedQueue = FormatQueue(opts.Queue, opts.MessageType);
            Logger.LogInformation("Delete Message {MessageId} messages from {FormattedQueue}.", opts.MessageId, formattedQueue);

            Logger.LogInformation("Attempting to delete message {MessageId}", opts.MessageId);

            var counts = GetQueue(opts);
            var count = formattedQueue.Contains("deadletter", StringComparison.CurrentCultureIgnoreCase) ? counts.DeadLetterMessageCount : counts.ActiveMessageCount;
            AmqpConnection conn = null;
            List<Amqp.Message> messages = new List<Amqp.Message>();
            Amqp.Message message;
            try {
                conn = Connect(opts);
                ReceiverLink receiver = new ReceiverLink(conn.Session, $"receiver-read", formattedQueue);
                receiver.SetCredit((int)count);
                TimeSpan timeSpan = TimeSpan.FromSeconds(10);
                while ((message = receiver.ReceiveAsync(timeSpan).GetAwaiter().GetResult()) != null) {
                    Logger.LogInformation("Reading message {MessageId}", message.Properties.MessageId);
                    if (message.Properties.MessageId == opts.MessageId) {
                        receiver.Accept(message);
                        success = true;
                        Logger.LogInformation("Successfully deleted message {MessageId}", message.Properties.MessageId);
                    } else {
                        messages.Add(message);
                    }
                }
                Logger.LogInformation("releasing {Count} messages", messages.Count);
                foreach (var msg in messages) {
                    receiver.Release(msg);
                }
                if (!success) {
                    throw new InvalidOperationException($"Message {opts.MessageId} could not be found.");
                }

                Logger.LogInformation("Closing connection");
                receiver.CloseAsync().GetAwaiter().GetResult();
                conn.Session.CloseAsync().GetAwaiter().GetResult();
                conn.Connection.CloseAsync().GetAwaiter().GetResult();
                Logger.LogInformation("Connection closed");
            } catch (Exception e) {
                if (null != conn?.Connection) {
                    conn.Connection.CloseAsync().GetAwaiter().GetResult();
                }
                Logger.LogError(e, "Error deleting message {MessageId} from queue {Queue}", opts.MessageId, opts.Queue);
                throw;
            }
            return success;
        }

        private string FormatQueue(string queue, string messageType) {
            Enum.TryParse(typeof(MessageType), messageType, true, out object type);
            switch (type) {
                case MessageType.Active:
                    return queue;
                case MessageType.DeadLetter:
                    return EntityNameHelper.FormatDeadLetterPath(queue);
                default:
                    // Not supported
                    throw new InvalidOperationException($"Peeking queues by type {messageType} is not supported");
            }
        }

        public enum MessageType {
            Active,
            DeadLetter,
            Scheduled
        }

        internal AmqpConnection Connect(DeleteMessageOptions options) {
            Logger.LogDebug("Connecting to {Url}.", options.GetUrl());
            try {
                Amqp.Address address = new Amqp.Address(options.GetUrl());
                var connection = new Connection(address);
                Session session = new Session(connection);

                Logger.LogInformation("Connection successfully established.");
                return new AmqpConnection() { Connection = connection, Session = session };
            } catch (Exception ex) {
                Logger.LogError(ex, "ServiceBusClient failed to establish connection.");
                throw;
            }
        }

        internal class AmqpConnection {
            public Connection Connection { get; set; }
            public Session Session { get; set; }
        }

        private ManagementClient GetClient(DeleteMessageOptions opts) => new ManagementClient(opts.GetConnectionString());

        private MessageCountDetails GetQueue(DeleteMessageOptions opts) {
            var managementClient = GetClient(opts);
            var messages = managementClient.GetQueueRuntimeInfoAsync(opts.Queue).GetAwaiter().GetResult();
            return messages.MessageCountDetails;
        }
    }
}
