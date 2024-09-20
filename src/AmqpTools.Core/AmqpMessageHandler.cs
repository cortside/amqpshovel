using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;
using Amqp;
using Amqp.Framing;
using AmqpTools.Core.Commands;
using Azure.Core.Amqp;
using Azure.Messaging.ServiceBus;
using Cortside.Common.Messages.MessageExceptions;
using Microsoft.Extensions.Logging;
using Message = Amqp.Message;

namespace AmqpTools.Core {
    public class AmqpMessageHandler {
        const string MESSAGE_TYPE_KEY = "Message.Type.FullName";
        private readonly ILogger logger;
        private readonly BaseOptions opts;

        public AmqpMessageHandler(ILogger logger, BaseOptions opts) {
            this.logger = logger;
            this.opts = opts;
        }

        public void Send(Message message) {
            Address address = new Address(opts.GetUrl());
            var connection = new Connection(address);
            Session session = new Session(connection);

            var attach = new Attach() {
                Target = new Target() { Address = opts.Queue, Durable = Convert.ToUInt32(opts.Durable) },
                Source = new Source()
            };
            var sender = new SenderLink(session, "shovel", attach, null);

            string rawBody = GetBody(message);

            // duplicate message so that original can be ack'd
            var m = new Message(rawBody) {
                Header = message.Header,
                ApplicationProperties = message.ApplicationProperties,
                Properties = message.Properties
            };

            logger.LogInformation("publishing message {MessageId} to {Queue} with event type {Message_Type_Key}", message.Properties.MessageId, opts.Queue, message.ApplicationProperties[MESSAGE_TYPE_KEY]);
            logger.LogDebug("Body for message {MessageId} is {Body}", message.Properties.MessageId, rawBody);

            try {
                sender.Send(m);
                logger.LogInformation("successfully published message {MessageId}", message.Properties.MessageId);
            } finally {
                if (sender.Error != null) {
                    logger.LogError("ERROR: [{Condition}] {Description}", sender.Error.Condition, sender.Error.Description);
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

        public static Message CreateMessage(string eventType, string data, string correlationId) {
            var messageId = Guid.NewGuid().ToString();
            var message = new Message(data) {
                Header = new Header {
                    Durable = false
                },
                ApplicationProperties = new ApplicationProperties(),
                MessageAnnotations = new MessageAnnotations(),
                Properties = new Properties {
                    MessageId = messageId,
                    GroupId = eventType,
                    CorrelationId = correlationId
                }
            };
            message.ApplicationProperties[MESSAGE_TYPE_KEY] = eventType;
            return message;
        }

        public static string GetBody(Message message) {
            if (message.Body == null) {
                return null;
            }

            string body = null;
            // Get the body
            if (message.Body is string s) {
                body = s;
            } else if (message.Body is byte[] bytes) {
                body = GetBody(bytes);
            } else {
                throw new InternalServerErrorResponseException($"Message {message.Properties.MessageId} has body with an invalid type {message.Body.GetType()}");
            }

            return body;
        }

        internal static string GetBody(ServiceBusReceivedMessage message) {
            AmqpAnnotatedMessage amqpMessage = message.GetRawAmqpMessage();
            if (amqpMessage.Body.TryGetValue(out object value)) {
                // handle the value body
                if (value is string s) {
                    return s;
                } else if (value is byte[] bytes) {
                    return GetBody(bytes);
                }
            } else if (amqpMessage.Body.TryGetSequence(out IEnumerable<IList<object>> sequence)) {
                // handle the sequence body
                Console.WriteLine("Got sequence");
            } else if (amqpMessage.Body.TryGetData(out IEnumerable<ReadOnlyMemory<byte>> bytes)) {
                // handle the data body - note that unlike when accessing the Body property of the received message,
                // we actually get back a list of byte arrays, not a single byte array. If you were to access the Body property,
                // the data would be flattened into a single byte array.
                Console.WriteLine("got bytes");
                return GetBody(ConvertToByteArray(bytes));
                //return GetBody(message.Body.ToArray());
            }

            return null;
        }

        private static byte[] ConvertToByteArray(IEnumerable<ReadOnlyMemory<byte>> readonlyData) {
            return readonlyData
                .SelectMany(memory => MemoryMarshal.AsBytes(memory.Span).ToArray())
                .ToArray();
        }

        private static string GetBody(byte[] bytes) {
            string body = null;

            if (bytes[0] == 64) {
                using (var reader = XmlDictionaryReader.CreateBinaryReader(new MemoryStream(bytes), null, XmlDictionaryReaderQuotas.Max)) {
                    var doc = new XmlDocument();
                    doc.Load(reader);
                    body = doc.InnerText;
                }
            } else {
                body = Encoding.UTF8.GetString(bytes);
            }
            return body;
        }


    }
}
