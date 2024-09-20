using Cortside.Common.Messages.MessageExceptions;

namespace AmqpTools.Core.Exceptions {
    public class InvalidArgumentMessageException : BadRequestResponseException {
        public InvalidArgumentMessageException() : base("The provided argument was not valid.") { }

        public InvalidArgumentMessageException(string message) : base(message) {
        }

        public InvalidArgumentMessageException(string message, System.Exception exception) : base(message, exception) {
        }
    }
}
