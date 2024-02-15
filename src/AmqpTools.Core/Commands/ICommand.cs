using Microsoft.Extensions.Logging;

namespace AmqpCommon.Commands {
    public interface ICommand {
        void ParseArguments(string[] args);
        int Execute();
        ILogger Logger { get; set; }
    }
}
