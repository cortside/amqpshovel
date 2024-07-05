using Microsoft.Extensions.Logging;

namespace AmqpTools.Core.Commands {
    public interface ICommand {
        void ParseArguments(string[] args);
        int Execute();
        ILogger Logger { get; set; }
    }
}
