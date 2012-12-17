using Website.Application.Queue;
using Website.Infrastructure.Command;

namespace Website.Application.Command
{
    public class QueuedCommandBus : CommandBusInterface
    {
        private readonly CommandSerializerInterface _commandSerializer;
        private readonly QueueInterface _queue;

        public QueuedCommandBus(CommandSerializerInterface commandSerializer, 
                               QueueInterface queue)
        {
            _commandSerializer = commandSerializer;
            _queue = queue;
        }

        #region Implementation of CommandBusInterface

        public object Send<TCommand>(TCommand command) where TCommand : class, CommandInterface
        {
            var msg = _commandSerializer.ToByteArray(command);
            var sendmessage = new QueueMessage(msg);
            _queue.AddMessage(sendmessage);
            return true;
        }

        #endregion
    }
}