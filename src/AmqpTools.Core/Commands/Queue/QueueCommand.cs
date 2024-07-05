using System;
using CommandLine;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AmqpTools.Core.Commands.Queue {
    public class QueueCommand : IServiceCommand<QueueOptions, MessageCountDetails> {
        private const int EXIT_SUCCESS = 0;
        const int ERROR_NO_MESSAGE = 1;
        const int ERROR_OTHER = 2;

        private ParserResult<QueueOptions> result;

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
                    var details = GetCountDetails(opts);
                    Console.Out.WriteLine(JsonConvert.SerializeObject(details));
                })
                .WithNotParsed(errors => {
                    foreach (var error in errors) {
                        Console.Out.WriteLine(error.ToString());
                    }
                });

            return EXIT_SUCCESS;
        }

        public MessageCountDetails ServiceExecute(QueueOptions options) {
            return GetCountDetails(options);
        }

        private MessageCountDetails GetCountDetails(QueueOptions opts) {
            var managementClient = new ManagementClient(opts.GetConnectionString());
            var queue = managementClient.GetQueueRuntimeInfoAsync(opts.Queue).GetAwaiter().GetResult();
            return queue.MessageCountDetails;
        }
    }
}
