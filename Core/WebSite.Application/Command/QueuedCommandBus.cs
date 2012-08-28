using Website.Infrastructure.Command;

namespace Website.Application.Command
{
    public class QueuedCommandBus : CommandBusInterface
    {
        private readonly CommandSerializerInterface _commandSerializer;
        private readonly QueueInterface _queue;
        private readonly MessageFactoryInterface _messageFactory;

        public QueuedCommandBus(CommandSerializerInterface commandSerializer, 
                               QueueInterface queue, MessageFactoryInterface messageFactory)
        {
            _commandSerializer = commandSerializer;
            _queue = queue;
            _messageFactory = messageFactory;
        }

        #region Implementation of CommandBusInterface

        public object Send<TCommand>(TCommand command) where TCommand : class, CommandInterface
        {
            var msg = _commandSerializer.ToByteArray(command);
            var sendmessage = _messageFactory.GetMessageForBytes(msg);
            _queue.AddMessage(sendmessage);
            return true;
        }

        #endregion
    }
}