using System;
using System.Collections.Generic;
using AmqpCommon;
using Azure.Messaging.ServiceBus.Administration;
using CommandLine;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AmqpQueue {
    public class Program {
        private const int EXIT_SUCCESS = 0;
        private static ILogger<Program> logger;

        public static void Main(string[] args) {
            CommandLine.Parser.Default.ParseArguments<QueueOptions>(args)
              .WithParsed<QueueOptions>(opts => Run(opts))
              .WithNotParsed<QueueOptions>((errs) => HandleParseError(errs));
        }

        private static void HandleParseError(IEnumerable<CommandLine.Error> errs) {
            foreach (var err in errs) {
                Console.Out.WriteLine(err.ToString());
            }
        }

        private static void Run(BaseOptions opts) {
            var loggerFactory = LoggerFactory.Create(builder => {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddConsole();
            });
            logger = loggerFactory.CreateLogger<Program>();
            var handler = new AmqpMessageHandler(loggerFactory.CreateLogger<AmqpMessageHandler>(), opts);

            GetCountDetails(handler, opts);
        }

        public static int GetCountDetails(AmqpMessageHandler handler, BaseOptions opts) {
            // https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/MigrationGuide.md

            var managementClient = new ServiceBusAdministrationClient(opts.ConnectionString);
            var queue = managementClient.GetQueueRuntimePropertiesAsync(opts.Queue).GetAwaiter().GetResult();

            // write to stdout for piping
            Console.Out.WriteLine(JsonConvert.SerializeObject(queue.Value));
            return EXIT_SUCCESS;
        }
    }
}
