using AmqpTools.Core.Commands.Shovel;

namespace AmqpTools.Core {
    public interface IAmqpToolsCore {

        void ShovelMessages(ShovelOptions options);

    }
}
