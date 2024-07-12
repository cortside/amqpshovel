using CommandLine;

namespace AmqpTools.Core.Commands.Peek {
    [Verb("peek", HelpText = "peeks at a queue for amqp messages")]
    public class PeekOptions : BaseOptions {
        [Option("count", Required = false, Default = 10, HelpText = "Count of messages to peek")]
        public int Count { get; set; }
        [Option("messageType", Required = true, HelpText = "Type of messages to peek (Active | DeadLetter)")]
        public string MessageType { get; set; }
    }
}
