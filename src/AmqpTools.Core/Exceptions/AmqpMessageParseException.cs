using Cortside.Common.Messages.MessageExceptions;

namespace AmqpTools.Core.Exceptions {
    public class AmqpMessageParseException : InternalServerErrorResponseException {
        public AmqpMessageParseException() : base("Failed to parse the message.") { }

        public AmqpMessageParseException(string message) : base(message) {
        }

        public AmqpMessageParseException(string message, System.Exception exception) : base(message, exception) {
        }
    }
}
