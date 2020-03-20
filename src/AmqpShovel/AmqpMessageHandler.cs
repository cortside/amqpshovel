using System;
using System.IO;
using System.Xml;
using Amqp;
using Amqp.Framing;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Message = Amqp.Message;

namespace AmqpShovel {
    class AmqpMessageHandler {
        const int ERROR_SUCCESS = 0;
        const int ERROR_NO_MESSAGE = 1;
        const int ERROR_OTHER = 2;
        const string MESSAGE_TYPE_KEY = "Message.Type.FullName";

        public static int ResendDeadLetters(ShovelOptions opts) {
            var dlq = EntityNameHelper.FormatDeadLetterPath(opts.Queue);
            var max = opts.Max;
            Console.Out.WriteLine($"Connecting to {opts.Queue} to shovel maximum of {max} messages");

            if (opts.ConnectionString != null) {
                var managementClient = new ManagementClient(opts.ConnectionString);
                var queue = managementClient.GetQueueRuntimeInfoAsync(opts.Queue).GetAwaiter().GetResult();
                var messageCount = queue.MessageCountDetails.DeadLetterMessageCount;
                Console.Out.WriteLine($"Message queue {dlq} has {messageCount} messages");

                if (messageCount < opts.Max) {
                    max = Convert.ToInt32(messageCount);
                    Console.Out.WriteLine($"resetting max messages to {max}");
                }
            }

            int exitCode = ERROR_SUCCESS;
            Connection connection = null;
            try {
                Address address = new Address(opts.Url);
                connection = new Connection(address);
                Session session = new Session(connection);
                ReceiverLink receiver = new ReceiverLink(session, "receiver-drain", dlq);

                Message message;
                int nReceived = 0;
                receiver.SetCredit(opts.InitialCredit);
                while ((message = receiver.Receive(opts.TimeSpan)) != null) {
                    nReceived++;
                    Console.WriteLine("Message(Properties={0}, ApplicationProperties={1}, Body={2}", message.Properties, message.ApplicationProperties, message.Body);
                    Send(opts, message);
                    receiver.Accept(message);
                    if (opts.Max > 0 && nReceived == max) {
                        Console.WriteLine("max messages received");
                        break;
                    }
                }
                if (message == null) {
                    Console.WriteLine("No message");
                    exitCode = ERROR_NO_MESSAGE;
                }
                receiver.Close();
                session.Close();
                connection.Close();
            } catch (Exception e) {
                Console.Error.WriteLine("Exception {0}.", e);
                if (null != connection) {
                    connection.Close();
                }
                exitCode = ERROR_OTHER;
            }

            Console.WriteLine("done");
            return exitCode;
        }

        public static void Send(ShovelOptions opts, Message message) {
            Address address = new Address(opts.Url);
            var connection = new Connection(address);
            Session session = new Session(connection);

            var attach = new Attach() {
                Target = new Target() { Address = opts.Queue, Durable = Convert.ToUInt32(opts.Durable) },
                Source = new Source()
            };
            var sender = new SenderLink(session, "shovel", attach, null);

            string rawBody = null;
            // Get the body
            if (message.Body is string) {
                rawBody = message.Body as string;
            } else if (message.Body is byte[]) {
                using (var reader = XmlDictionaryReader.CreateBinaryReader(
                    new MemoryStream(message.Body as byte[]),
                    null,
                    XmlDictionaryReaderQuotas.Max)) {
                    var doc = new XmlDocument();
                    doc.Load(reader);
                    rawBody = doc.InnerText;
                }
            }

            // duplicate message so that original can be ack'd
            var m = new Message(rawBody) {
                Header = message.Header,
                ApplicationProperties = message.ApplicationProperties,
                Properties = message.Properties
            };

            try {
                sender.Send(m);
            } finally {
                if (sender.Error != null) {
                    Console.WriteLine($"ERROR: [{sender.Error.Condition}] {sender.Error.Description}");
                }
                if (!sender.IsClosed) {
                    sender.Close(TimeSpan.FromSeconds(5));
                }
                session.Close();
                session.Connection.Close();
            }
            if (sender.Error != null) {
                throw new AmqpException(sender.Error);
            }
        }
    }
}
