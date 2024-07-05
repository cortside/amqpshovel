namespace AmqpTools.Core.Commands {
    public interface IServiceCommand<in TOptions, out TResult> : ICommand {
        TResult ServiceExecute(TOptions options);

    }
}
