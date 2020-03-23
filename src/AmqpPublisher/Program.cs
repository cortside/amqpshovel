using System;
using System.Collections.Generic;
using System.IO;
using AmqpCommon;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace AmqpPublisher {

    public class Program {

        public static void Main(string[] args) {
            CommandLine.Parser.Default.ParseArguments<PublisherOptions>(args)
              .WithParsed<PublisherOptions>(opts => Run(opts))
              .WithNotParsed<PublisherOptions>((errs) => HandleParseError(errs));
        }

        private static void HandleParseError(IEnumerable<Error> errs) {
            foreach (var err in errs) {
                Console.WriteLine($"ERROR: {err}");
            }
        }

        private static void Run(PublisherOptions opts) {
            var loggerFactory = LoggerFactory.Create(builder => {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddConsole();
            });
            ILogger logger = loggerFactory.CreateLogger<Program>();
            var handler = new AmqpMessageHandler(loggerFactory.CreateLogger<AmqpMessageHandler>(), opts);

            if (!String.IsNullOrWhiteSpace(opts.File)) {
                opts.Data = File.ReadAllText(opts.File);
            }

            if (string.IsNullOrEmpty(opts.Data)) {
                logger.LogWarning("Data or File must be specified and have data");
            }

            var message = AmqpMessageHandler.CreateMessage(opts.EventType, opts.Data, null);
            bool success;
            try {
                handler.Send(message);
                success = true;
            } catch (Exception ex) {
                logger.LogError(ex, "Error publishing message");
                success = false;
            }
            if (success) {
                logger.LogInformation("message sent");
            }
        }
    }
}
