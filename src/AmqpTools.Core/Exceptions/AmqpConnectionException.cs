using Cortside.Common.Messages.MessageExceptions;

namespace AmqpTools.Core.Exceptions {
    public class AmqpConnectionException : InternalServerErrorResponseException {
        public AmqpConnectionException() : base("Could not establish a connection to the server.") { }

        public AmqpConnectionException(string message) : base(message) {
        }

        public AmqpConnectionException(string message, System.Exception exception) : base(message, exception) {
        }
    }
}
