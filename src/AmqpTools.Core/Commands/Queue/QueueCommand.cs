using System;
using System.Linq;
using AmqpTools.Core.Exceptions;
using AmqpTools.Core.Models;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CommandLine;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AmqpTools.Core.Commands.Queue {
    public class QueueCommand : ICommand, IServiceCommand<QueueOptions, AmqpToolsQueueRuntimeInfo> {
        private const int EXIT_SUCCESS = 0;
        const int ERROR_NO_MESSAGE = 1;
        const int ERROR_OTHER = 2;

        private ParserResult<QueueOptions> result;

        public QueueCommand() {
            if (GetType().GetConstructor(Type.EmptyTypes) == null) {
                throw new InvalidProgramException("Parameterless constructor required.");
            }
        }

        public ILogger Logger { get; set; }

        public void ParseArguments(string[] args, Configuration config) {
            result = Parser.Default.ParseArguments<QueueOptions>(args);
            result.WithParsed(opts => {
                opts.ApplyConfig();
            });

            if (!string.IsNullOrWhiteSpace(result.Value.Environment) && config.Environments.Exists(x => x.Name == result.Value.Environment)) {
                var env = config.Environments.First(x => x.Name == result.Value.Environment);
                result.Value.Namespace ??= env.Namespace;
                result.Value.PolicyName ??= env.PolicyName;
                result.Value.Key ??= env.Key;
            }
        }

        public int Execute() {
            Logger.LogInformation($"Connecting to {result.Value.Namespace} as policy {result.Value.PolicyName} for queue {result.Value.Queue}");

            result
                .WithParsed(opts => {
                    var details = GetRuntimeInfo(opts);
                    Console.Out.WriteLine(JsonConvert.SerializeObject(details));
                })
                .WithNotParsed(errors => {
                    foreach (var error in errors) {
                        Console.Out.WriteLine(error.ToString());
                    }
                });

            return EXIT_SUCCESS;
        }

        public AmqpToolsQueueRuntimeInfo ServiceExecute(QueueOptions options) {
            return GetRuntimeInfo(options);
        }

        private AmqpToolsQueueRuntimeInfo GetRuntimeInfo(QueueOptions opts) {
            try {
                var adminClient = new ServiceBusAdministrationClient(opts.GetConnectionString());
                var queue = adminClient.GetQueueRuntimePropertiesAsync(opts.Queue).GetAwaiter().GetResult();
                return Map(queue.Value);
            } catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound) {
                Logger.LogError(ex, "Error getting queue info: {Message}", ex.Message);
                throw new NotFoundResponseException($"Queue not found {opts.Queue}");
            } catch (Exception ex) {
                Logger.LogError(ex, "Error getting queue runtime info {Message}", ex.Message);
                throw new AmqpConnectionException();
            }
        }

        private AmqpToolsQueueRuntimeInfo Map(QueueRuntimeProperties queue) {
            return new AmqpToolsQueueRuntimeInfo {
                Path = queue.Name,
                MessageCount = queue.TotalMessageCount,
                MessageCountDetails = new AmqpToolsMessageCountDetails {
                    ActiveMessageCount = queue.ActiveMessageCount,
                    DeadLetterMessageCount = queue.DeadLetterMessageCount,
                    ScheduledMessageCount = queue.ScheduledMessageCount,
                    TransferMessageCount = queue.TransferMessageCount,
                    TransferDeadLetterMessageCount = queue.TransferDeadLetterMessageCount,
                },
                SizeInBytes = queue.SizeInBytes,
                CreatedAt = queue.CreatedAt.DateTime,
                UpdatedAt = queue.UpdatedAt.DateTime,
                AccessedAt = queue.AccessedAt.DateTime
            };
        }
    }
}
