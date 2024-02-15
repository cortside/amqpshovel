using System;
using Amqp;
using CommandLine;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;

namespace AmqpCommon.Commands.Shovel {
    public class ShovelCommand : ICommand {
        const int ERROR_SUCCESS = 0;
        const int ERROR_NO_MESSAGE = 1;
        const int ERROR_OTHER = 2;

        private ParserResult<ShovelOptions> result;

        public ILogger Logger { get; set; }

        public void ParseArguments(string[] args) {
            result = Parser.Default.ParseArguments<ShovelOptions>(args);
            result.WithParsed(opts => {
                opts.ApplyConfig();
            });
        }

        public int Execute() {
            result
                .WithParsed(opts => {
                    var handler = new AmqpMessageHandler(Logger, opts);

                    var dlq = EntityNameHelper.FormatDeadLetterPath(opts.Queue);
                    var max = opts.Max;
                    Logger.LogInformation($"Connecting to {opts.Queue} to shovel maximum of {max} messages");

                    if (opts.GetConnectionString() != null) {
                        var managementClient = new ManagementClient(opts.GetConnectionString());
                        var queue = managementClient.GetQueueRuntimeInfoAsync(opts.Queue).GetAwaiter().GetResult();
                        var messageCount = queue.MessageCountDetails.DeadLetterMessageCount;
                        Logger.LogInformation($"Message queue {dlq} has {messageCount} messages");

                        if (messageCount < opts.Max) {
                            max = Convert.ToInt32(messageCount);
                            Logger.LogInformation($"resetting max messages to {max}");
                        }
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
                        receiver.SetCredit(opts.InitialCredit);
                        while ((message = receiver.Receive(opts.GetTimeSpan())) != null) {
                            nReceived++;
                            var body = AmqpMessageHandler.GetBody(message);
                            Logger.LogInformation("Message(Properties={0}, ApplicationProperties={1}, Body={2}", message.Properties, message.ApplicationProperties, body);

                            // TODO: should have option to skip messages that are not valid -- i.e. don't have a type
                            handler.Send(message);
                            receiver.Accept(message);
                            if (opts.Max > 0 && nReceived == max) {
                                Logger.LogInformation("max messages received");
                                break;
                            }
                        }
                        if (message == null) {
                            Logger.LogInformation("No message");
                            exitCode = ERROR_NO_MESSAGE;
                        }
                        receiver.Close();
                        session.Close();
                        connection.Close();
                    } catch (Exception e) {
                        Logger.LogError(e, "Exception {0}.");
                        if (null != connection) {
                            connection.Close();
                        }
                        exitCode = ERROR_OTHER;
                    }

                    Logger.LogInformation("done");
                })
                .WithNotParsed(errors => {
                    foreach (var error in errors) {
                        Console.Out.WriteLine(error.ToString());
                    }
                });

            return ERROR_SUCCESS;
        }
    }
}
