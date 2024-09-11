using CommandLine;

namespace AmqpTools.Core.Commands.Shovel {
    [Verb("shovel", HelpText = "shovels deadletterqueue for a queue")]
    public class ShovelOptions : BaseOptions {
        [Option(Default = 100, HelpText = "Maximum dlq messages to process")]
        public int Max { get; set; }
        [Option("messageId", Required = false, HelpText = "Id of message to shovel - overrides Max value")]
        public string MessageId { get; set; }
    }
}
