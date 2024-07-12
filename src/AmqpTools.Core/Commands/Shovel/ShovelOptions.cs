using CommandLine;

namespace AmqpTools.Core.Commands.Shovel {
    public class ShovelOptions : BaseOptions {
        public ShovelOptions() { }

        [Option(Default = 100, HelpText = "Maximum dlq messages to process")]
        public int Max { get; set; }
    }
}
