using System;
using AmqpTools.Core.Commands;
using AmqpTools.Core.Commands.Publish;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AmqpTools.Test {
    public class CommandFactoryTest : BaseTest {
        private readonly ILoggerFactory loggerFactory;

        public CommandFactoryTest() : base() {
            loggerFactory = LoggerFactory.Create(builder => {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddConsole();
            });
        }

        [Fact]
        public void ShouldThrowOnUnknownCommand() {
            // Act & assert
            new CommandFactory().Invoking(x => x.CreateCommand(loggerFactory, new string[] { "blah" }))
                .Should().Throw<ArgumentException>()
                .WithMessage("unknown command blah (Parameter 'name')");
        }

        [Fact]
        public void ShouldCreateCommand() {
            // Act
            var command = new CommandFactory().CreateCommand(loggerFactory, new string[] { "publish" });

            // assert
            command.Should().NotBeNull();
            command.Should().BeOfType<PublishCommand>();
            command.Logger.Should().NotBeNull();

        }
    }
}
