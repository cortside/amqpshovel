using System;
using System.Collections.Generic;
using System.Linq;
using AmqpTools.Core.Models;
using CommandLine;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AmqpTools.Core.Commands.Peek {
    public class PeekCommand : IServiceCommand<PeekOptions, IList<AmqpToolsMessage>> {
        private const int EXIT_SUCCESS = 0;
        const int ERROR_NO_MESSAGE = 1;
        const int ERROR_OTHER = 2;

        private ParserResult<PeekOptions> result;

        public ILogger Logger { get; set; }

        public void ParseArguments(string[] args) {
            result = Parser.Default.ParseArguments<PeekOptions>(args);
            result.WithParsed(opts => {
                opts.ApplyConfig();
            });

        }

        public int Execute() {
            result
            .WithParsed(opts => {
                var messages = PeekMessages(opts);
                foreach (var message in messages) {
                    Console.Out.WriteLine(JsonConvert.SerializeObject(message));
                }
            })
                .WithNotParsed(errors => {
                    foreach (var error in errors) {
                        Console.Out.WriteLine(error.ToString());
                    }
                });

            return EXIT_SUCCESS;
        }

        public IList<AmqpToolsMessage> ServiceExecute(PeekOptions options) {
            return PeekMessages(options);
        }

        private IList<AmqpToolsMessage> PeekMessages(PeekOptions opts) {
            string formattedQueue = FormatQueue(opts.Queue, opts.MessageType);
            Logger.LogInformation("Peeking {Count} messages from {FormattedQueue}.", opts.Count, formattedQueue);

            var receiver = new MessageReceiver(opts.GetConnectionString(), formattedQueue, ReceiveMode.PeekLock);
            bool success;
            var messages = new List<Message>();
            try {
                messages = receiver.PeekAsync(opts.Count).GetAwaiter().GetResult().ToList();

                success = true;
                receiver.CloseAsync().GetAwaiter().GetResult();

            } catch (Exception ex) {
                Logger.LogError(ex, "Error peeking messages");
                success = false;
            }
            if (success) {
                Logger.LogInformation("messages peeked");
            }
            return messages.ConvertAll(m => Map(m));
        }

        private AmqpToolsMessage Map(Message message) {
            return new AmqpToolsMessage {
                Body = message.GetBody<string>(),
                MessageId = message.MessageId,
                CorrelationId = message.CorrelationId,
                PartitionKey = message.PartitionKey,
                ExpiresAtUtc = message.ExpiresAtUtc,
                ContentType = message.ContentType,
                ScheduledEnqueueTimeUtc = message.ScheduledEnqueueTimeUtc,
                UserProperties = message.UserProperties,
                SystemProperties = message.SystemProperties
            };
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
    }
}
