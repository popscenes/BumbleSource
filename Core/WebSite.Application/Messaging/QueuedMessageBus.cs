using Website.Application.Queue;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;

namespace Website.Application.Messaging
{
    public class QueuedMessageBus : MessageBusInterface
    {
        private readonly MessageSerializerInterface _messageSerializer;
        private readonly QueueInterface _queue;

        public QueuedMessageBus(MessageSerializerInterface messageSerializer, 
                               QueueInterface queue)
        {
            _messageSerializer = messageSerializer;
            _queue = queue;
        }

        #region Implementation of CommandBusInterface

        public object Send<TCommand>(TCommand command) where TCommand : class, CommandInterface
        {
            var msg = _messageSerializer.ToByteArray(command);
            var sendmessage = new QueueMessage(msg);
            _queue.AddMessage(sendmessage);
            return true;
        }

        #endregion
    }
}