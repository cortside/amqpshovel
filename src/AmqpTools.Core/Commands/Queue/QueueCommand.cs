using System;
using AmqpTools.Core.Exceptions;
using AmqpTools.Core.Models;
using CommandLine;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AmqpTools.Core.Commands.Queue {
    [Verb("queue", HelpText = "gets runtime info for a queue")]
    public class QueueCommand : ICommand, IServiceCommand<QueueOptions, AmqpToolsQueueRuntimeInfo> {
        private const int EXIT_SUCCESS = 0;
        const int ERROR_NO_MESSAGE = 1;
        const int ERROR_OTHER = 2;

        private ParserResult<QueueOptions> result;

        public QueueCommand() {
            if (GetType().GetConstructor(Type.EmptyTypes) == null)
                throw new InvalidProgramException("Parameterless constructor required.");
        }

        public ILogger Logger { get; set; }

        public void ParseArguments(string[] args) {
            result = Parser.Default.ParseArguments<QueueOptions>(args);
            result.WithParsed(opts => {
                opts.ApplyConfig();
            });

        }

        public int Execute() {
            //// https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/MigrationGuide.md
            //var managementClient = new ServiceBusAdministrationClient(opts.GetConnectionString());
            //var queue = managementClient.GetQueueRuntimePropertiesAsync(opts.Queue).GetAwaiter().GetResult();
            //// write to stdout for piping
            //Console.Out.WriteLine(JsonConvert.SerializeObject(queue.Value));

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

                var managementClient = new ManagementClient(opts.GetConnectionString());
                var queue = managementClient.GetQueueRuntimeInfoAsync(opts.Queue).GetAwaiter().GetResult();
                return Map(queue);
            } catch (MessagingEntityNotFoundException ex) {
                Logger.LogError(ex, "Error getting queue runtime info {Message}", ex.Message);
                throw new NotFoundResponseException($"Queue not found {opts.Queue}");
            } catch (Exception ex) {
                Logger.LogError(ex, "Error getting queue runtime info {Message}", ex.Message);
                throw new AmqpConnectionException();
            }
        }

        private AmqpToolsQueueRuntimeInfo Map(QueueRuntimeInfo queue) {
            return new AmqpToolsQueueRuntimeInfo {
                Path = queue.Path,
                MessageCount = queue.MessageCount,
                MessageCountDetails = new AmqpToolsMessageCountDetails {
                    ActiveMessageCount = queue.MessageCountDetails.ActiveMessageCount,
                    DeadLetterMessageCount = queue.MessageCountDetails.DeadLetterMessageCount,
                    ScheduledMessageCount = queue.MessageCountDetails.ScheduledMessageCount,
                    TransferMessageCount = queue.MessageCountDetails.TransferMessageCount,
                    TransferDeadLetterMessageCount = queue.MessageCountDetails.TransferDeadLetterMessageCount
                },
                SizeInBytes = queue.SizeInBytes,
                CreatedAt = queue.CreatedAt,
                UpdatedAt = queue.UpdatedAt,
                AccessedAt = queue.AccessedAt
            };
        }
    }
}
