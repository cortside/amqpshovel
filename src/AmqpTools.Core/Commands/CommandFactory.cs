using System;
using System.Collections.Generic;
using AmqpCommon.Commands.Publish;
using AmqpCommon.Commands.Queue;
using AmqpCommon.Commands.Shovel;
using Microsoft.Extensions.Logging;

namespace AmqpCommon.Commands {
    public class CommandFactory {
        private readonly Dictionary<string, Type> commands;

        public CommandFactory() {
            commands = new Dictionary<string, Type>() {
                {"queue", typeof(QueueCommand)},
                {"shovel", typeof(ShovelCommand)},
                {"publish", typeof(PublishCommand)}
            };
        }

        public ICommand CreateCommand(ILoggerFactory loggerFactory, string[] args) {
            var name = args[0];
            if (!commands.ContainsKey(name)) {
                throw new ArgumentException($"unknown command {name}", nameof(name));
            }

            var type = commands[name];
            var command = Activator.CreateInstance(type) as ICommand;
            command.Logger = loggerFactory.CreateLogger(type);
            command.ParseArguments(args);

            return command;
        }
    }
}
