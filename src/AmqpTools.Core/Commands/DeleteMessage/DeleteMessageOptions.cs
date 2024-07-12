using CommandLine;

namespace AmqpTools.Core.Commands.DeleteMessage {
    [Verb("delete", HelpText = "deletes a message from a queue")]
    public class DeleteMessageOptions : BaseOptions {
        [Option("messageId", Required = true, HelpText = "Id of message to delete")]
        public string MessageId { get; set; }
        [Option("messageType", Required = true, HelpText = "Type of messages to peek (Active | DeadLetter)")]
        public string MessageType { get; set; }
    }
}
