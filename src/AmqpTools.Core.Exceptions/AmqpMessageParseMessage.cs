using Cortside.Common.Messages.MessageExceptions;

namespace AmqpTools.Core.Exceptions {
    public class AmqpMessageParseMessage : InternalServerErrorResponseException {
        public AmqpMessageParseMessage() : base("Failed to parse the message.") { }

        public AmqpMessageParseMessage(string message) : base(message) {
        }

        public AmqpMessageParseMessage(string message, System.Exception exception) : base(message, exception) {
        }
    }
}
