using System;
using AmqpTools.Core.Commands;
using Microsoft.Extensions.Logging;

namespace AmqpTools {
    public class Program {
        private static ILogger<Program> logger;

        public static int Main(string[] args) {
            if (args.Length == 0) {
                throw new ArgumentException("Must pass arguments", nameof(args));
            }

            var loggerFactory = LoggerFactory.Create(builder => {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddConsole();
            });

            var command = new CommandFactory().CreateCommand(loggerFactory, args);
            if (command != null) {
                return command.Execute();
            }

            return 1;
        }
    }
}
