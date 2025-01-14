﻿using System;
using System.Collections.Generic;
using AmqpTools.Core.Commands.DeleteMessage;
using AmqpTools.Core.Commands.Peek;
using AmqpTools.Core.Commands.Publish;
using AmqpTools.Core.Commands.Queue;
using AmqpTools.Core.Commands.Shovel;
using Microsoft.Extensions.Logging;

namespace AmqpTools.Core.Commands {
    public class CommandFactory {
        private readonly Dictionary<string, Type> commands;

        public CommandFactory() {
            commands = new Dictionary<string, Type>() {
                {"queue", typeof(QueueCommand)},
                {"shovel", typeof(ShovelCommand)},
                {"peek", typeof(PeekCommand)},
                {"delete", typeof(DeleteMessageCommand)},
                {"publish", typeof(PublishCommand)}
            };
        }

        public ICommand CreateCommand(ILoggerFactory loggerFactory, string[] args, Configuration config) {
            var name = args[0];
            if (!commands.TryGetValue(name, out Type value)) {
                throw new ArgumentException($"unknown command {name}", nameof(name));
            }

            var type = value;
            var command = Activator.CreateInstance(type) as ICommand;
            command.Logger = loggerFactory.CreateLogger(type);
            command.ParseArguments(args, config);

            return command;
        }

        public IServiceCommand<TOptions, TResult> CreateCommand<TOptions, TResult>(ILoggerFactory loggerFactory, Type type) {
            if (!commands.ContainsValue(type)) {
                throw new InvalidOperationException($"unknown command type {nameof(type)}");
            }

            var command = Activator.CreateInstance(type) as IServiceCommand<TOptions, TResult>;
            command.Logger = loggerFactory.CreateLogger(type);

            return command;
        }
    }
}
