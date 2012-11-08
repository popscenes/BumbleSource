using Website.Application.Command;

namespace Website.Application.ApplicationCommunication
{
    public class DefaultApplicationBroadcastCommunicatorFactory : ApplicationBroadcastCommunicatorFactoryInterface
    {
        private readonly CommandQueueFactoryInterface _commandQueueFactory;
        private readonly ApplicationBroadcastCommunicatorRegistrationInterface _applicationBroadcastCommunicatorRegistration;

        public DefaultApplicationBroadcastCommunicatorFactory(CommandQueueFactoryInterface commandQueueFactory
                                                   , ApplicationBroadcastCommunicatorRegistrationInterface applicationBroadcastCommunicatorRegistration)
        {
            _commandQueueFactory = commandQueueFactory;
            _applicationBroadcastCommunicatorRegistration = applicationBroadcastCommunicatorRegistration;
        }

        public ApplicationBroadcastCommunicatorInterface GetCommunicatorForEndpoint(string endpoint)
        {
            return new DefaultApplicationBroadcastCommunicator(endpoint, _commandQueueFactory, _applicationBroadcastCommunicatorRegistration);
        }
    }
}