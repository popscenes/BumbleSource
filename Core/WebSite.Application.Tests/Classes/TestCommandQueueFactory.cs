using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Util;

namespace Website.Application.Tests.Classes
{
    public class TestCommandQueueFactory : CommandQueueFactoryInterface
    {
        private readonly MoqMockingKernel _kernel;

        public TestCommandQueueFactory(MoqMockingKernel kernel)
        {
            _kernel = kernel;
        }

        public class TestQueueMessage : QueueMessageInterface
        {
            public byte[] Bytes { get; set; }
        }

        public class TestMessageFactory : MessageFactoryInterface
        {
            public QueueMessageInterface GetMessageForBytes(byte[] bytes)
            {
                return new TestQueueMessage(){Bytes = bytes};
            }
        }

        public class TestCommandSerializer : CommandSerializerInterface
        {
            public Action<string, object> CommandSerializerActionCallback { get; set; }

            public CommandType FromByteArray<CommandType>(byte[] array) where CommandType : class, CommandInterface
            {
                var ret = SerializeUtil.FromByteArray(array) as CommandType;
                Callback("FromByteArray", ret);
                return ret;
            }

            public byte[] ToByteArray<CommandType>(CommandType command) where CommandType : class, CommandInterface
            {
                var ret = SerializeUtil.ToByteArray(command);
                Callback("ToByteArray", command);
                return ret;
            }

            public void ReleaseCommand<CommandType>(CommandType command) where CommandType : class, CommandInterface
            {
                Callback("ReleaseCommand", command);
            }

            private void Callback(string methodName, object cmd)
            {
                if (CommandSerializerActionCallback != null)
                    CommandSerializerActionCallback(methodName, cmd);
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

            public ConcurrentQueue<QueueMessageInterface> Storage
            {
                get { return _messages;}
            }

            private void Callback(string endPoint, string methodName, QueueMessageInterface queueMessage)
            {
                if(QueueActionCallback != null)
                    QueueActionCallback(endPoint, methodName, queueMessage);
            }
        }

        private readonly ConcurrentDictionary<string, TestQueue> _queues = new ConcurrentDictionary<string, TestQueue>();
        private readonly TestCommandSerializer _commandSerializer = new TestCommandSerializer();
        private readonly TestMessageFactory _messageFactory = new TestMessageFactory();
        private Action<string, string, QueueMessageInterface> _queueActionCallback;

        public CommandBusInterface GetCommandBusForEndpoint(string queueEndpoint)
        {
            return new QueuedCommandBus(
                _commandSerializer,
                GetQueue(queueEndpoint),
                _messageFactory
                );
        }

        public void Delete(string queueEndpoint)
        {
            TestQueue currentQueue;
            _queues.TryRemove(queueEndpoint, out currentQueue);
        }

        public QueuedCommandProcessor GetSchedulerForEndpoint(string queueEndpoint)
        {
            return new QueuedCommandProcessor(GetQueue(queueEndpoint)
                                              , _commandSerializer
                                              , _kernel.Get<CommandHandlerRespositoryInterface>());
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
            _commandSerializer.CommandSerializerActionCallback = commandSerializerActionCallback;
        }

        public IList<TestQueue> GetTestQueues()
        {
            return _queues.Select(kv => kv.Value).ToList();
        }
    }
}