using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using AmqpTools.Core.Exceptions;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace AmqpTools.Core.Commands.Publish {
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
    public class PublishCommand : ICommand {
        private const int EXIT_SUCCESS = 0;
        const int ERROR_NO_MESSAGE = 1;
        const int ERROR_OTHER = 2;

        private ParserResult<PublishOptions> result;

        public ILogger Logger { get; set; }

        public PublishCommand() { }

        public void ParseArguments(string[] args, Configuration config) {
            result = Parser.Default.ParseArguments<PublishOptions>(args);
            result.WithParsed(opts => {
                opts.ApplyConfig();
            });

            if (!string.IsNullOrWhiteSpace(result.Value.Environment) && config.Environments.Exists(x => x.Name == result.Value.Environment)) {
                var env = config.Environments.First(x => x.Name == result.Value.Environment);
                result.Value.Namespace ??= env.Namespace;
                result.Value.PolicyName ??= env.PolicyName;
                result.Value.Key ??= env.Key;
            }
        }

        public int Execute() {
            Logger.LogInformation($"Connecting to {result.Value.Namespace} as policy {result.Value.PolicyName} for queue {result.Value.Queue}");

            result
                .WithParsed(opts => {
                    var handler = new AmqpMessageHandler(Logger, opts);

                    if (!string.IsNullOrWhiteSpace(opts.File)) {
                        opts.Data = File.ReadAllText(opts.File);
                    }

                    if (string.IsNullOrEmpty(opts.Data)) {
                        Logger.LogError("Data or File must be specified and have data");
                        throw new InvalidArgumentMessageException($"Data or File option must be specified and have data");
                    }

                    var message = AmqpMessageHandler.CreateMessage(opts.EventType, opts.Data, null);
                    bool success;
                    try {
                        handler.Send(message);
                        success = true;
                    } catch (Exception ex) {
                        Logger.LogError(ex, "Error publishing message");
                        success = false;
                    }
                    if (success) {
                        Logger.LogInformation("message sent");
                    }
                })
                .WithNotParsed(errors => {
                    foreach (var error in errors) {
                        Console.Out.WriteLine(error.ToString());
                    }
                });

            return EXIT_SUCCESS;
        }
    }
}
