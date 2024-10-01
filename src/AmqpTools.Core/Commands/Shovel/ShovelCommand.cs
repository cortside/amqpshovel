using System;
using System.Collections.Generic;
using System.Linq;
using Amqp;
using AmqpTools.Core.Exceptions;
using AmqpTools.Core.Util;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CommandLine;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.Extensions.Logging;

namespace AmqpTools.Core.Commands.Shovel {
    public class ShovelCommand : ICommand, IServiceCommand<ShovelOptions, int> {
        const int ERROR_SUCCESS = 0;
        const int ERROR_NO_MESSAGE = 1;
        const int ERROR_OTHER = 2;

        private ParserResult<ShovelOptions> result;

        public ILogger Logger { get; set; }

        public ShovelCommand() { }

        public void ParseArguments(string[] args, Configuration config) {
            result = Parser.Default.ParseArguments<ShovelOptions>(args);
            result.WithParsed(opts => {
                opts.ApplyConfig();
            });

            if (!string.IsNullOrWhiteSpace(result.Value.Environment) && config.Environments.Exists(x => x.Name == result.Value.Environment)) {
                var env = config.Environments.First(x => x.Name == result.Value.Environment);
                Logger.LogInformation("Environment {Env} found in config, using environment settings", env.Name);
                result.Value.Namespace ??= env.Namespace;
                result.Value.PolicyName ??= env.PolicyName;
                result.Value.Key ??= env.Key;
                result.Value.Protocol ??= env.Protocol;
            }
        }

        public int Execute() {
            Logger.LogInformation($"Connecting to {result.Value.Namespace} as policy {result.Value.PolicyName} for queue {result.Value.Queue}");

            var exitCode = ERROR_SUCCESS;
            result
                .WithParsed(opts => {
                    exitCode = Shovel(opts);

                })
                .WithNotParsed(errors => {
                    foreach (var error in errors) {
                        Console.Out.WriteLine(error.ToString());
                    }
                    exitCode = ERROR_OTHER;
                });

            return exitCode;
        }

        public int ServiceExecute(ShovelOptions options) {
            return Shovel(options);
        }

        private int Shovel(ShovelOptions opts) {
            var handler = new AmqpMessageHandler(Logger, opts);

            var dlq = EntityNameHelper.FormatDeadLetterPath(opts.Queue);
            var max = opts.Max;
            Logger.LogInformation("Connecting to {Queue} to shovel maximum of {Max} messages", opts.Queue, max);
            try {
                if (opts.GetConnectionString() != null) {
                    var adminClient = new ServiceBusAdministrationClient(opts.GetConnectionString());
                    var queue = adminClient.GetQueueRuntimePropertiesAsync(opts.Queue).GetAwaiter().GetResult();
                    var messageCount = queue.Value.DeadLetterMessageCount;
                    Logger.LogInformation("Message queue {Dlq} has {MessageCount} messages", dlq, messageCount);

                    if (messageCount < opts.Max) {
                        max = Convert.ToInt32(messageCount);
                        Logger.LogInformation("resetting max messages to {Max}", max);
                    }
                }
            } catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound) {
                Logger.LogError(ex, "Error getting queue info: {Message}", ex.Message);
                throw new NotFoundResponseException($"Queue not found {opts.Queue}");
            } catch (Exception ex) {
                Logger.LogError(ex, "Error getting queue runtime info {Message}", ex.Message);
                throw new AmqpShovelException();
            }

            int exitCode = ERROR_SUCCESS;
            Connection connection = null;
            try {
                Address address = new Address(opts.GetUrl());
                connection = new Connection(address);
                Session session = new Session(connection);
                ReceiverLink receiver = new ReceiverLink(session, "receiver-drain", dlq);

                Amqp.Message message;
                int nReceived = 0;
                var timeout = opts.GetTimeSpan();
                receiver.SetCredit(max);
                var useMessageId = !string.IsNullOrEmpty(opts.MessageId);
                List<Amqp.Message> messagesToRelease = new List<Amqp.Message>();

                while ((message = receiver.Receive(timeout)) != null) {
                    nReceived++;
                    var body = AmqpMessageHandler.GetBody(message);
                    Logger.LogInformation("Reading message {MessageId}", message.Properties.MessageId);
                    Logger.LogDebug("Message(Properties={0}, ApplicationProperties={1}, Body={2}", message.Properties, message.ApplicationProperties, body);

                    if (body != null && useMessageId && message.Properties.MessageId == opts.MessageId) {
                        Logger.LogInformation("Shoveling single message {MessageId}", opts.MessageId);
                        handler.Send(message);
                        receiver.Accept(message);
                    } else if (useMessageId) {
                        messagesToRelease.Add(message);
                    }

                    // TODO: should have option to skip messages that are not valid -- i.e. don't have a type
                    if (body != null && !useMessageId) {
                        handler.Send(message);
                        receiver.Accept(message);
                    }

                    if (opts.Max > 0 && nReceived == max) {
                        Logger.LogInformation("max messages received");
                        break;
                    }
                }

                Logger.LogInformation("releasing {Count} messages", messagesToRelease.Count);
                foreach (var msg in messagesToRelease) {
                    Logger.LogDebug("Releasing message {MessageId}, it is not the one to be shoveled", msg.Properties.MessageId);
                    receiver.Release(msg);
                }

                if (message == null) {
                    Logger.LogInformation("No message");
                    exitCode = ERROR_NO_MESSAGE;
                }
                receiver.Close();
                session.Close();
                connection.Close();
            } catch (Exception e) {
                Logger.LogError(e, "Exception {0}.", e.Message);
                if (null != connection) {
                    connection.Close();
                }
                exitCode = ERROR_OTHER;
                throw new AmqpShovelException();
            }

            Logger.LogInformation("done");
            return exitCode;
        }
    }
}
