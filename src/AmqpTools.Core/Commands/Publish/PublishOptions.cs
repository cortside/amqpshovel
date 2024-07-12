using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace AmqpTools.Core.Commands.Publish {
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
    [Verb("publish", HelpText = "publishes an amqp message")]
    public class PublishOptions : BaseOptions {
        public PublishOptions() { }

        [Option("data", Required = false, HelpText = "Message data/json")]
        public string Data { get; set; }
        [Option("file", Required = false, HelpText = "filename for Message data/json")]
        public string File { get; set; }
        [Option("eventtype", Required = true, HelpText = "Event type (event class full name)")]
        public string EventType { get; set; }
    }
}
