using System;
using AmqpTools.Core.Commands;
using AmqpTools.Core.Commands.DeleteMessage;
using AmqpTools.Core.Commands.Peek;
using AmqpTools.Core.Commands.Publish;
using AmqpTools.Core.Commands.Queue;
using AmqpTools.Core.Commands.Shovel;
using Microsoft.Extensions.Logging;

namespace AmqpTools {
    public class Program {
        private static ILogger<Program> logger;
        public static int Main(string[] args) {
            Type[] types = new Type[] { typeof(PublishCommand), typeof(QueueCommand), typeof(PeekCommand), typeof(DeleteMessageCommand), typeof(ShovelCommand) };
            // either way requires binding options to command classes for the commandlineparser to successfully parse - but moot, since its parser is not compatable with trimming for  aot
            //var result = CommandLine.Parser.Default.ParseArguments(args, types);
            //var result = CommandLine.Parser.Default.ParseArguments<PublishCommand, QueueCommand, PeekCommand, DeleteMessageCommand, ShovelCommand>(args);
            //result.WithParsed(x => {
            //    Console.WriteLine($"parsed type result: {x}");
            //})
            //    .WithNotParsed(x => {
            //        Console.WriteLine("command type not parsed");
            //    });
            //.WithParsed(opts => Run(opts))
            //  .WithNotParsed<PublisherOptions>((errs) => HandleParseError(errs));

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
