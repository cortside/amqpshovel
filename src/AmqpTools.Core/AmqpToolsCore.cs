using System;
using System.Collections.Generic;
using AmqpTools.Core.Commands;
using AmqpTools.Core.Commands.DeleteMessage;
using AmqpTools.Core.Commands.Peek;
using AmqpTools.Core.Commands.Queue;
using AmqpTools.Core.Commands.Shovel;
using AmqpTools.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AmqpTools.Core {
    public class AmqpToolsCore : IAmqpToolsCore {

        private readonly ILogger logger;

        private readonly ILoggerFactory factory;

        public AmqpToolsCore(ILogger<AmqpToolsCore> logger, IServiceProvider serviceProvider) {
            this.logger = logger;
            factory = serviceProvider.GetRequiredService<ILoggerFactory>();

        }

        public void ShovelMessages(ShovelOptions options) {
            logger.LogDebug("Creating ShovelCommand");
            var command = new CommandFactory().CreateCommand<ShovelOptions, int>(factory, typeof(ShovelCommand));
            if (command != null) {
                logger.LogDebug("Executing Shovel as Service");

                command.ServiceExecute(options);

                logger.LogDebug("Done executing Shovel as Service");
            }
        }

        public AmqpToolsQueueRuntimeInfo GetQueueRuntimeInfo(QueueOptions options) {
            logger.LogDebug("Creating QueueCommand");
            var command = new CommandFactory().CreateCommand<QueueOptions, AmqpToolsQueueRuntimeInfo>(factory, typeof(QueueCommand));
            if (command != null) {
                logger.LogDebug("getting queue message counts as Service");

                var result = command.ServiceExecute(options);

                logger.LogDebug("Done getting queue message counts as Service");
                return result;
            }
            return null;
        }

        public IList<AmqpToolsMessage> PeekMessages(PeekOptions options) {
            logger.LogDebug("Creating PeekCommand");
            var command = new CommandFactory().CreateCommand<PeekOptions, IList<AmqpToolsMessage>>(factory, typeof(PeekCommand));
            if (command != null) {
                logger.LogDebug("peeking messages as Service");

                var result = command.ServiceExecute(options);

                logger.LogDebug("Done peeking messages as Service");
                return result;
            }
            return new List<AmqpToolsMessage>();
        }

        public bool DeleteMessage(DeleteMessageOptions options) {
            logger.LogDebug("Creating DeleteCommand");
            var command = new CommandFactory().CreateCommand<DeleteMessageOptions, bool>(factory, typeof(DeleteMessageCommand));
            if (command != null) {
                logger.LogDebug("Deleteing messages as Service");

                var result = command.ServiceExecute(options);

                logger.LogDebug("Done Deleteing messages as Service");
                return result;
            }
            return false;
        }
    }
}
