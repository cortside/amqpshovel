using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CommandLine;
using Newtonsoft.Json;

namespace AmqpTools.Core.Commands {
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
    public class BaseOptions {
        [Option('e', "environment", Required = false, HelpText = "Environment", SetName = "environment")]
        public string Environment { get; set; }

        [Option('q', "Queue", Required = true, HelpText = "Queue")]
        public string Queue { get; set; }

        [Option(Default = 10)]
        public int InitialCredit { get; set; }

        [Option(Default = 1)]
        public double Timeout { get; set; }

        [Option('n', "namespace", Required = false, HelpText = "Namespace to connect to", SetName = "configuration")]
        public string Namespace { get; set; }

        [Option('k', "key", Required = false, HelpText = "Key to connect to namespace", SetName = "configuration")]
        public string Key { get; set; }

        [Option('p', "policyname", Required = false, HelpText = "Policy for key used to connect to namespace", SetName = "configuration")]
        public string PolicyName { get; set; }

        [Option(Default = "amqps", SetName = "configuration")]
        public string Protocol { get; set; }

        [Option(Default = 1)]
        public int Durable { get; set; }

        [Option("config", Default = "amqptools.json", Required = false, HelpText = "filename for Message data/json")]
        public string Config { get; set; }

        //private string Url => $"{Protocol}://{PolicyName}:{Key}@{Namespace}/";

        //private string ConnectionString {
        //    get {
        //        if (Namespace.Contains("windows.net")) {
        //            return $"Endpoint=sb://{Namespace}/;SharedAccessKeyName={PolicyName};SharedAccessKey={Key}";
        //        }
        //        return null;
        //    }
        //}

        //private TimeSpan TimeSpan {
        //    get {
        //        TimeSpan timeout = TimeSpan.MaxValue;
        //        if (Timeout != 0) {
        //            timeout = TimeSpan.FromSeconds(Timeout);
        //        }
        //        return timeout;
        //    }
        //}

        public string GetUrl() {
            return $"{Protocol}://{PolicyName}:{Key}@{Namespace}/";
        }

        public string GetConnectionString() {
            if (Namespace.Contains("windows.net")) {
                return $"Endpoint=sb://{Namespace}/;SharedAccessKeyName={PolicyName};SharedAccessKey={Key}";
            }
            return null;
        }

        public TimeSpan GetTimeSpan() {
            TimeSpan timeout = TimeSpan.MaxValue;
            if (Timeout != 0) {
                timeout = TimeSpan.FromSeconds(Timeout);
            }
            return timeout;
        }

        public void ApplyConfig() {
            if (File.Exists(Config)) {
                var s = File.ReadAllText(Config);
                var json = JsonConvert.DeserializeObject<BaseOptions>(s);
                Namespace ??= json.Namespace;
                PolicyName ??= json.PolicyName;
                Key ??= json.Key;
                Queue ??= json.Queue;
            }
        }
    }
}
