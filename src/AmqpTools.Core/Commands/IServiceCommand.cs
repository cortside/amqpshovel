using Microsoft.Extensions.Logging;

namespace AmqpTools.Core.Commands {
    public interface IServiceCommand<in TOptions, out TResult> {
        TResult ServiceExecute(TOptions options);
        ILogger Logger { get; set; }

    }
}
