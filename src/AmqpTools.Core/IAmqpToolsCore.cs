using System.Collections.Generic;
using AmqpTools.Core.Commands.DeleteMessage;
using AmqpTools.Core.Commands.Peek;
using AmqpTools.Core.Commands.Queue;
using AmqpTools.Core.Commands.Shovel;
using AmqpTools.Core.Models;

namespace AmqpTools.Core {
    public interface IAmqpToolsCore {

        bool DeleteMessage(DeleteMessageOptions options);

        AmqpToolsQueueRuntimeInfo GetQueueRuntimeInfo(QueueOptions options);

        void ShovelMessages(ShovelOptions options);

        IList<AmqpToolsMessage> PeekMessages(PeekOptions options);
    }
}
