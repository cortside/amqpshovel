using System;
using System.Collections.Generic;
using System.Linq;
using AmqpTools.Core.Exceptions;
using AmqpTools.Core.Models;
using AmqpTools.Core.Util;
using Azure.Messaging.ServiceBus;
using CommandLine;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AmqpTools.Core.Commands.Peek {
    public class PeekCommand : ICommand, IServiceCommand<PeekOptions, IList<AmqpToolsMessage>> {
        private const int EXIT_SUCCESS = 0;
        const int ERROR_NO_MESSAGE = 1;
        const int ERROR_OTHER = 2;

        private ParserResult<PeekOptions> result;

        public ILogger Logger { get; set; }

        public void ParseArguments(string[] args, Configuration config) {
            result = Parser.Default.ParseArguments<PeekOptions>(args);
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
            Logger.LogInformation("Connecting to {Namespace} as policy {PolicyName} for queue {Queue}", result.Value.Namespace, result.Value.PolicyName, result.Value.Queue);

            result
                .WithParsed(opts => {
                    var messages = PeekMessages(opts);
                    foreach (var message in messages) {
                        Logger.LogDebug(JsonConvert.SerializeObject(message));
                    }
                })
                .WithNotParsed(errors => {
                    foreach (var error in errors) {
                        Logger.LogInformation(error.ToString());
                    }
                });

            return EXIT_SUCCESS;
        }

        public IList<AmqpToolsMessage> ServiceExecute(PeekOptions options) {
            return PeekMessages(options);
        }

        private IList<AmqpToolsMessage> PeekMessages(PeekOptions opts) {
            string formattedQueue = EntityNameHelper.FormatQueue(opts.Queue, opts.MessageType);
            Logger.LogInformation("Peeking {Count} messages from {FormattedQueue}.", opts.Count, formattedQueue);

            ServiceBusClient client = new(opts.GetConnectionString());

            List<ServiceBusReceivedMessage> messages;
            try {
                var receiver = client.CreateReceiver(formattedQueue, new ServiceBusReceiverOptions {
                    ReceiveMode = ServiceBusReceiveMode.PeekLock
                });

                messages = receiver.PeekMessagesAsync(opts.Count).GetAwaiter().GetResult().ToList();

                receiver.CloseAsync().GetAwaiter().GetResult();
            } catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound) {
                Logger.LogError(ex, "Error peeking messages: {Message}", ex.Message);
                throw new NotFoundResponseException($"Queue not found {opts.Queue}");
            } catch (Exception ex) {
                Logger.LogError(ex, "Error peeking messages: {Message}", ex.Message);
                throw new AmqpPeekException();
            }

            Logger.LogInformation("messages peeked");
            return messages.ConvertAll(m => Map(m));
        }

        private AmqpToolsMessage Map(ServiceBusReceivedMessage message) {
            string body = AmqpMessageHandler.GetBody(message);

            if (body != null) {
                List<string> errors = new List<string>();
                JsonConvert.DeserializeObject(body,
                    new JsonSerializerSettings {
                        Error = (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) => {
                            errors.Add(args.ErrorContext.Error.Message);
                            args.ErrorContext.Handled = true;
                        }
                    });
                if (errors.Count > 0) {
                    Logger.LogWarning("Message {MessageId} has errors deserializing message body: {Errors}",
                        message.MessageId, string.Join(", ", errors));
                }
            }

            var raw = message.GetRawAmqpMessage();
            var systemProperties = raw.DeliveryAnnotations.Concat(raw.MessageAnnotations
                .Where(kvp => !raw.DeliveryAnnotations.ContainsKey(kvp.Key)))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return new AmqpToolsMessage {
                Body = body,
                MessageId = message.MessageId,
                CorrelationId = message.CorrelationId,
                PartitionKey = message.PartitionKey,
                ExpiresAtUtc = message.ExpiresAt.DateTime,
                ContentType = message.ContentType,
                ScheduledEnqueueTimeUtc = message.ScheduledEnqueueTime.DateTime,
                UserProperties = message.ApplicationProperties,
                SystemProperties = systemProperties,
            };
        }


    }
}
