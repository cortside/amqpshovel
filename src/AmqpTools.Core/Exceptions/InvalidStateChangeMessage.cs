using Cortside.Common.Messages.MessageExceptions;

namespace AmqpTools.Core.Exceptions {
    public class InvalidStateChangeMessage : UnprocessableEntityResponseException {
        public InvalidStateChangeMessage() : base("Current state does not allow requested operation.") { }
        public InvalidStateChangeMessage(string message) : base($"Current state does not allow requested operation. {message}") { }
    }
}
