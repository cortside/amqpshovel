using CommandLine;

namespace AmqpTools.Core.Commands.Peek {
    public class DeleteMessageOptions : BaseOptions {
        [Option("messageId", Required = true, HelpText = "Id of message to delete")]
        public string MessageId { get; set; }
        [Option("messageType", Required = true, HelpText = "Type of messages to peek (Active | DeadLetter)")]
        public string MessageType { get; set; }
    }
}
