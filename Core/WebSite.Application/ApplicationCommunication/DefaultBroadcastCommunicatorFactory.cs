using Website.Application.Command;

namespace Website.Application.ApplicationCommunication
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