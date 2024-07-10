using Cortside.Common.Messages.MessageExceptions;

namespace AmqpTools.Core.Exceptions {
    public class AmqpConnectionMessage : InternalServerErrorResponseException {
        public AmqpConnectionMessage() : base("Could not establish a connection to the server.") { }

        public AmqpConnectionMessage(string message) : base(message) {
        }

        public AmqpConnectionMessage(string message, System.Exception exception) : base(message, exception) {
        }
    }
}
