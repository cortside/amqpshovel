using System.Collections.Generic;

namespace AmqpTools.Core.Commands {
    public class Configuration {
        public List<Environment> Environments { get; set; } = new List<Environment>();
    }
}
