using System;
using System.IO;
using System.Xml;
using Amqp;
using Amqp.Framing;
using Microsoft.Extensions.Logging;
using Message = Amqp.Message;

namespace AmqpCommon {
    public class AmqpMessageHandler {
        const string MESSAGE_TYPE_KEY = "Message.Type.FullName";
        private readonly ILogger<AmqpMessageHandler> logger;
        private readonly BaseOptions opts;

        public AmqpMessageHandler(ILogger<AmqpMessageHandler> logger, BaseOptions opts) {
            this.logger = logger;
            this.opts = opts;
        }

        public void Send(Message message) {
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

            logger.LogInformation($"publishing message {message.Properties.MessageId} to {opts.Queue} with event type {message.ApplicationProperties[MESSAGE_TYPE_KEY]} with body:\n{rawBody}");

            try {
                sender.Send(m);
                logger.LogInformation($"successfully published message {message.Properties.MessageId}");
            } finally {
                if (sender.Error != null) {
                    logger.LogError($"ERROR: [{sender.Error.Condition}] {sender.Error.Description}");
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
    }
}
