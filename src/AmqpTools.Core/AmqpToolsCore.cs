using System;
using AmqpTools.Core.Commands;
using AmqpTools.Core.Commands.Shovel;
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
            var command = new CommandFactory().CreateCommand<ShovelOptions>(factory, typeof(ShovelCommand));
            if (command != null) {
                logger.LogDebug("Executing Shovel as Service");

                command.ServiceExecute(options);

                logger.LogDebug("Done executing Shovel as Service");
            }
        }
    }
}
