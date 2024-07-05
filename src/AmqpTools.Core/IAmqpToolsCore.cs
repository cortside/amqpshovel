using System.Collections.Generic;
using AmqpTools.Core.Commands.Peek;
using AmqpTools.Core.Commands.Queue;
using AmqpTools.Core.Commands.Shovel;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace AmqpTools.Core {
    public interface IAmqpToolsCore {
        MessageCountDetails GetQueueCountDetails(QueueOptions options);

        void ShovelMessages(ShovelOptions options);

        IList<Message> PeekMessages(PeekOptions options);
    }
}
