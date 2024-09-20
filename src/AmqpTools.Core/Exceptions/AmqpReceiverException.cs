using Cortside.Common.Messages.MessageExceptions;

namespace AmqpTools.Core.Exceptions {
    public class AmqpReceiverException : InternalServerErrorResponseException {
        public AmqpReceiverException() : base("Failed to receive messages.") { }

        public AmqpReceiverException(string message) : base(message) {
        }

        public AmqpReceiverException(string message, System.Exception exception) : base(message, exception) {
        }
    }
}
