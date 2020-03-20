using System;
using CommandLine;

namespace AmqpShovel {
    public class ShovelOptions {
        [Option('q', "queue", Required = true, HelpText = "Queue")]
        public string Queue { get; internal set; }

        [Option(Default = 1, HelpText = "Maximum concurrent threads")]
        public int MaxConcurrentCalls { get; internal set; }

        [Option(Default = 100, HelpText = "Maximum dlq messages to process")]
        public int Max { get; internal set; }

        [Option(Default = 10)]
        public int InitialCredit { get; internal set; }

        [Option(Default = 1)]
        public double Timeout { get; internal set; }


        [Option(Default = "cortside-test.servicebus.windows.net")]
        public string Namespace { get; internal set; }

        [Option(Default = "secret")]
        public string Key { get; internal set; }

        [Option(Default = "RootManageSharedAccessKey")]
        public string PolicyName { get; internal set; }

        [Option(Default = "amqps")]
        public string Protocol { get; internal set; }

        [Option(Default = 1)]
        public int Durable { get; internal set; }



        public bool Forever => Timeout == 0;

        public string Url => $"{Protocol}://{PolicyName}:{Key}@{Namespace}/";

        public string ConnectionString {
            get {
                if (Namespace.Contains("windows.net")) {
                    return $"Endpoint=sb://{Namespace}/;SharedAccessKeyName={PolicyName};SharedAccessKey={Key}";
                }
                return null;
            }
        }

        public TimeSpan TimeSpan {
            get {
                TimeSpan timeout = TimeSpan.MaxValue;
                if (!Forever) {
                    timeout = TimeSpan.FromSeconds(Timeout);
                }
                return timeout;
            }
        }
    }
}
