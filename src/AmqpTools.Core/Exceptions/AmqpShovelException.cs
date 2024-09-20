using Cortside.Common.Messages.MessageExceptions;

namespace AmqpTools.Core.Exceptions {
    public class AmqpShovelException : InternalServerErrorResponseException {
        public AmqpShovelException() : base("Failed to shovel messages.") { }

        public AmqpShovelException(string message) : base(message) {
        }

        public AmqpShovelException(string message, System.Exception exception) : base(message, exception) {
        }
    }
}
