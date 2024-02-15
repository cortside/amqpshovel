using System;
using System.Collections.Generic;
using AmqpCommon;
using AmqpCommon.Commands;
using CommandLine;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AmqpQueue {
    public class Program {
        private const int EXIT_SUCCESS = 0;
        private static ILogger<Program> logger;

        public static void Main(string[] args) {
            Parser.Default.ParseArguments<BaseOptions>(args)
                .WithParsed(Run)
                .WithNotParsed(HandleParseError);
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
            //// https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/servicebus/Azure.Messaging.ServiceBus/MigrationGuide.md

            //var managementClient = new ServiceBusAdministrationClient(opts.GetConnectionString());
            //var queue = managementClient.GetQueueRuntimePropertiesAsync(opts.Queue).GetAwaiter().GetResult();

            //// write to stdout for piping
            //Console.Out.WriteLine(JsonConvert.SerializeObject(queue.Value));

            var managementClient = new ManagementClient(opts.GetConnectionString());
            var queue = managementClient.GetQueueRuntimeInfoAsync(opts.Queue).GetAwaiter().GetResult();

            // write to stdout for piping
            Console.Out.WriteLine(JsonConvert.SerializeObject(queue.MessageCountDetails));

            return EXIT_SUCCESS;
        }
    }
}
