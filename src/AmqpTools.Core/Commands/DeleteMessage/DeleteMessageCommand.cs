using System;
using System.Collections.Generic;
using System.Linq;
using Amqp;
using AmqpTools.Core.Exceptions;
using AmqpTools.Core.Util;
using Azure.Messaging.ServiceBus.Administration;
using CommandLine;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.Extensions.Logging;

namespace AmqpTools.Core.Commands.DeleteMessage {
    public class DeleteMessageCommand : ICommand, IServiceCommand<DeleteMessageOptions, bool> {
        private const int EXIT_SUCCESS = 0;
        const int ERROR_NO_MESSAGE = 1;
        const int ERROR_OTHER = 2;

        private ParserResult<DeleteMessageOptions> result;

        public ILogger Logger { get; set; }

        public void ParseArguments(string[] args, Configuration config) {
            result = Parser.Default.ParseArguments<DeleteMessageOptions>(args);
            result.WithParsed(opts => {
                opts.ApplyConfig();
            });

            if (!string.IsNullOrWhiteSpace(result.Value.Environment) && config.Environments.Exists(x => x.Name == result.Value.Environment)) {
                var env = config.Environments.First(x => x.Name == result.Value.Environment);
                Logger.LogInformation("Environment {Env} found in config, using environment settings", env.Name);
                result.Value.Namespace ??= env.Namespace;
                result.Value.PolicyName ??= env.PolicyName;
                result.Value.Key ??= env.Key;
            }
        }

        public int Execute() {
            Logger.LogInformation($"Connecting to {result.Value.Namespace} as policy {result.Value.PolicyName} for queue {result.Value.Queue}");

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

            string formattedQueue = EntityNameHelper.FormatQueue(opts.Queue, opts.MessageType);
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
                    throw new NotFoundResponseException($"Message {opts.MessageId} could not be found.");
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


        internal AmqpConnection Connect(DeleteMessageOptions options) {
            Logger.LogDebug("Connecting to {Url}.", options.Namespace);
            try {
                Address address = new Address(options.GetUrl());
                var connection = new Connection(address);
                Session session = new Session(connection);

                Logger.LogInformation("Connection successfully established.");
                return new AmqpConnection() { Connection = connection, Session = session };
            } catch (Exception ex) {
                Logger.LogError(ex, "ServiceBusClient failed to establish connection.");
                throw new AmqpConnectionException();
            }
        }

        internal class AmqpConnection {
            public Connection Connection { get; set; }
            public Session Session { get; set; }
        }

        private QueueRuntimeProperties GetQueue(DeleteMessageOptions opts) {
            try {
                var adminClient = new ServiceBusAdministrationClient(opts.GetConnectionString());
                var queue = adminClient.GetQueueRuntimePropertiesAsync(opts.Queue).GetAwaiter().GetResult();
                return queue.Value;

            } catch (Exception ex) {
                Logger.LogError(ex, "Queue not found {Queue}", opts.Queue);
                throw new NotFoundResponseException($"Queue not found {opts.Queue}");
            }
        }
    }
}
