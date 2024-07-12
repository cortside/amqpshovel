using System;
using AmqpTools.Core.Commands;
using AmqpTools.Core.Commands.DeleteMessage;
using AmqpTools.Core.Commands.Peek;
using AmqpTools.Core.Commands.Publish;
using AmqpTools.Core.Commands.Queue;
using AmqpTools.Core.Commands.Shovel;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace AmqpTools {
    public class Program {
        private static ILogger<Program> logger;
        public static int Main(string[] args) {
            var result = CommandLine.Parser.Default.ParseArguments<PeekOptions, PublishOptions, QueueOptions, DeleteMessageOptions, ShovelOptions>(args);
            result.WithParsed(x => {
                Console.WriteLine($"parsed type result: {x}");

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
                    command.Execute();
                }
            })
            .WithNotParsed(x => {
                Console.WriteLine("command type not parsed");
            });


            return 1;
        }

    }
}
