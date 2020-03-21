using System;
using System.Collections.Generic;
using Amqp;
using AmqpCommon;
using CommandLine;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;

namespace AmqpShovel {

    public class Program {
        const int ERROR_SUCCESS = 0;
        const int ERROR_NO_MESSAGE = 1;
        const int ERROR_OTHER = 2;
        private static ILogger<Program> logger;

        public static void Main(string[] args) {
            CommandLine.Parser.Default.ParseArguments<ShovelOptions>(args)
              .WithParsed<ShovelOptions>(opts => Run(opts))
              .WithNotParsed<ShovelOptions>((errs) => HandleParseError(errs));
        }

        private static void HandleParseError(IEnumerable<CommandLine.Error> errs) {
            foreach (var err in errs) {
                Console.Out.WriteLine(err.ToString());
            }
        }

        private static void Run(BaseOptions opts) {
            var loggerFactory = LoggerFactory.Create(builder => {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddConsole();
            });
            logger = loggerFactory.CreateLogger<Program>();
            var handler = new AmqpMessageHandler(loggerFactory.CreateLogger<AmqpMessageHandler>(), opts);

            logger.LogInformation($"Shoveling messages for {EntityNameHelper.FormatDeadLetterPath(opts.Queue)}");
            ResendDeadLetters(handler, opts);
            logger.LogInformation("Messages shoveled");
        }

        public static int ResendDeadLetters(AmqpMessageHandler handler, BaseOptions opts) {
            var dlq = EntityNameHelper.FormatDeadLetterPath(opts.Queue);
            var max = opts.Max;
            logger.LogInformation($"Connecting to {opts.Queue} to shovel maximum of {max} messages");

            if (opts.ConnectionString != null) {
                var managementClient = new ManagementClient(opts.ConnectionString);
                var queue = managementClient.GetQueueRuntimeInfoAsync(opts.Queue).GetAwaiter().GetResult();
                var messageCount = queue.MessageCountDetails.DeadLetterMessageCount;
                logger.LogInformation($"Message queue {dlq} has {messageCount} messages");

                if (messageCount < opts.Max) {
                    max = Convert.ToInt32(messageCount);
                    logger.LogInformation($"resetting max messages to {max}");
                }
            }

            int exitCode = ERROR_SUCCESS;
            Connection connection = null;
            try {
                Address address = new Address(opts.Url);
                connection = new Connection(address);
                Session session = new Session(connection);
                ReceiverLink receiver = new ReceiverLink(session, "receiver-drain", dlq);

                Amqp.Message message;
                int nReceived = 0;
                receiver.SetCredit(opts.InitialCredit);
                while ((message = receiver.Receive(opts.TimeSpan)) != null) {
                    nReceived++;
                    logger.LogInformation("Message(Properties={0}, ApplicationProperties={1}, Body={2}", message.Properties, message.ApplicationProperties, message.Body);
                    handler.Send(message);
                    receiver.Accept(message);
                    if (opts.Max > 0 && nReceived == max) {
                        logger.LogInformation("max messages received");
                        break;
                    }
                }
                if (message == null) {
                    logger.LogInformation("No message");
                    exitCode = ERROR_NO_MESSAGE;
                }
                receiver.Close();
                session.Close();
                connection.Close();
            } catch (Exception e) {
                logger.LogError(e, "Exception {0}.");
                if (null != connection) {
                    connection.Close();
                }
                exitCode = ERROR_OTHER;
            }

            logger.LogInformation("done");
            return exitCode;
        }
    }
}
