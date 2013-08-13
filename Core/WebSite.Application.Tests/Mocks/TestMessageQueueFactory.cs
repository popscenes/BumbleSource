using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Messaging;
using Website.Application.Queue;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Types;
using Website.Infrastructure.Util;

namespace Website.Application.Tests.Mocks
{
    public class TestMessageQueueFactory : MessageQueueFactoryInterface
    {
        private readonly MoqMockingKernel _kernel;

        public TestMessageQueueFactory(MoqMockingKernel kernel)
        {
            _kernel = kernel;
        }

        public class TestQueueMessage : QueueMessageInterface
        {
            public byte[] Bytes { get; set; }
            public string CorrelationId { get; set; }
        }

        public class TestMessageFactory
        {
            public QueueMessageInterface GetMessageForBytes(byte[] bytes)
            {
                return new TestQueueMessage(){Bytes = bytes};
            }
        }

        public class TestMessageSerializer : MessageSerializerInterface
        {
            public Action<string, object> CommandSerializerActionCallback { get; set; }

            public CommandType FromByteArray<CommandType>(byte[] array) where CommandType : class, MessageInterface
            {
                var ret = SerializeUtil.FromByteArray(array) as CommandType;
                Callback("FromByteArray", ret);
                return ret;
            }

            public byte[] ToByteArray<CommandType>(CommandType command) where CommandType : class, MessageInterface
            {
                var ret = SerializeUtil.ToByteArray(command);
                Callback("ToByteArray", command);
                return ret;
            }

            public void ReleaseCommand<CommandType>(CommandType command) where CommandType : class, MessageInterface
            {
                Callback("ReleaseCommand", command);
            }

            private void Callback(string methodName, object cmd)
            {
                if (CommandSerializerActionCallback != null)
                    CommandSerializerActionCallback(methodName, cmd);
            }
        }

        private readonly ConcurrentDictionary<string, TestQueue> _queues = new ConcurrentDictionary<string, TestQueue>();
        private readonly TestMessageSerializer _messageSerializer = new TestMessageSerializer();
        private readonly TestMessageFactory _messageFactory = new TestMessageFactory();
        private Action<string, string, QueueMessageInterface> _queueActionCallback;

        public MessageBusInterface GetMessageBusForEndpoint(string queueEndpoint)
        {
            return new QueuedMessageBus(
                _messageSerializer,
                GetQueue(queueEndpoint)
                );
        }

        public void Delete(string queueEndpoint)
        {
            TestQueue currentQueue;
            _queues.TryRemove(queueEndpoint, out currentQueue);
        }

        public QueuedMessageProcessor GetProcessorForEndpoint(string queueEndpoint)
        {
            return new QueuedMessageProcessor(GetQueue(queueEndpoint)
                                              , _messageSerializer
                                              , _kernel.Get<MessageHandlerRespositoryInterface>());
        }

        private TestQueue GetQueue(string queueEndpoint)
        {
            TestQueue currentQueue;
            if (!_queues.TryGetValue(queueEndpoint, out currentQueue))
            {
                currentQueue = new TestQueue() { EndpointName = queueEndpoint, QueueActionCallback = _queueActionCallback};
                _queues[queueEndpoint] = currentQueue;
            }
            return currentQueue;
        }

        public ConcurrentQueue<QueueMessageInterface> GetStorageForTestEndpoint(string queueEndpoint)
        {
            return GetQueue(queueEndpoint).Storage;
        }

        public void AddQueueActionListener(Action<string, string, QueueMessageInterface> queueActionCallback)
        {
            foreach (var testQueue in _queues)
            {
                testQueue.Value.QueueActionCallback = queueActionCallback;
            }
            _queueActionCallback = queueActionCallback;
        }

        public void AddCmdSerializerListener(Action<string, object> commandSerializerActionCallback)
        {
            _messageSerializer.CommandSerializerActionCallback = commandSerializerActionCallback;
        }

        public IList<TestQueue> GetTestQueues()
        {
            return _queues.Select(kv => kv.Value).ToList();
        }
    }

    public class TestQueue : QueueInterface
    {
        public string EndpointName { get; set; }
        public Action<string, string, QueueMessageInterface> QueueActionCallback { get; set; }

        private readonly ConcurrentQueue<QueueMessageInterface> _messages = new ConcurrentQueue<QueueMessageInterface>();
        public void AddMessage(QueueMessageInterface message)
        {
            _messages.Enqueue(message);
            Callback(EndpointName, "AddMessage", message);
        }

        public QueueMessageInterface GetMessage()
        {
            QueueMessageInterface ret;
            _messages.TryDequeue(out ret);
            Callback(EndpointName, "GetMessage", ret);
            return ret;
        }

        public QueueMessageInterface GetMessage(TimeSpan invisibilityTimeOut)
        {
            return GetMessage();
        }

        public void DeleteMessage(QueueMessageInterface message)
        {
            //not implemented because get message removes message (ie simulates invisibility of the message)
            Callback(EndpointName, "DeleteMessage", message);
        }

        public int? ApproximateMessageCount { get { return _messages.Count; } }

        public ConcurrentQueue<QueueMessageInterface> Storage
        {
            get { return _messages;}
        }

        private void Callback(string endPoint, string methodName, QueueMessageInterface queueMessage)
        {
            if(QueueActionCallback != null)
                QueueActionCallback(endPoint, methodName, queueMessage);
        }

        public void Clear()
        {
           while (!_messages.IsEmpty)
           {
               QueueMessageInterface ret;
               _messages.TryDequeue(out ret);
           }
        }
    }
}