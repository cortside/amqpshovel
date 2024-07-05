namespace AmqpTools.Core.Commands.Shovel {
    public interface IServiceCommand<TOptions> : ICommand {
        object ServiceExecute(TOptions options);

    }
}
