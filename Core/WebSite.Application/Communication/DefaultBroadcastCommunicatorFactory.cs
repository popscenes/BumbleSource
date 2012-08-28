using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Website.Application.Command;

namespace Website.Application.Communication
{
    public class DefaultBroadcastCommunicatorFactory : BroadcastCommunicatorFactoryInterface
    {
        private readonly CommandQueueFactoryInterface _commandQueueFactory;
        private readonly BroadcastRegistratorInterface _broadcastRegistrator;

        public DefaultBroadcastCommunicatorFactory(CommandQueueFactoryInterface commandQueueFactory
                                                   , BroadcastRegistratorInterface broadcastRegistrator)
        {
            _commandQueueFactory = commandQueueFactory;
            _broadcastRegistrator = broadcastRegistrator;
        }

        public BroadcastCommunicatorInterface GetCommunicatorForEndpoint(string endpoint)
        {
            return new DefaultBroadcastCommunicator(endpoint, _commandQueueFactory, _broadcastRegistrator);
        }
    }
}