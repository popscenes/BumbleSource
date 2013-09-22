using Ninject;
using Ninject.Syntax;
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

        public void Send<TCommand>(TCommand command) where TCommand : class, CommandInterface
        {
            var msg = _messageSerializer.ToByteArray(command);
            var sendmessage = new QueueMessage(msg);
            _queue.AddMessage(sendmessage);
        }

        #endregion
    }

    public class TopcQueuedMessageBus : MessageBusInterface
    {
        private readonly MessageSerializerInterface _messageSerializer;
        private readonly QueueInterface _queue;
        private readonly IResolutionRoot _resolutionRoot;

        public TopcQueuedMessageBus(MessageSerializerInterface messageSerializer,
                               QueueInterface queue)
        {
            _messageSerializer = messageSerializer;
            _queue = queue;
        }

        #region Implementation of CommandBusInterface

        public void Send<TCommand>(TCommand command) where TCommand : class, CommandInterface
        {
            var msgbus = _resolutionRoot.Get<MessageBusInterface>(metadata => metadata.Get<string>("Topic") == command.GetType().Name);
            msgbus.Send(command);
        }

        #endregion
    }
}