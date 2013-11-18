using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Application.Messaging;
using Website.Application.Queue;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;

namespace Website.Application.Tests.Mocks
{

    /*public class TestEventTopicSenderFactory : EventTopicSenderFactoryInterface
    {
        private readonly ConcurrentDictionary<string, TestEventTopicSender> _queues =
            new ConcurrentDictionary<string, TestEventTopicSender>();

        public EventTopicSenderInterface GetTopicSender(Type type)
        {
            return GetTopicSender(type.Name.Replace("Event", ""));
        }

        public EventTopicSenderInterface GetTopicSender(string name)
        {
            TestEventTopicSender eventTopicSender = null;
            if (!_queues.TryGetValue(name, out eventTopicSender))
            {
                eventTopicSender = new TestEventTopicSender(name);
                _queues[name] = eventTopicSender;
            }
            return eventTopicSender;
        }
    }*/


    public class TestEventTopicSender : EventTopicSenderInterface
    {
        //private List<QueueMessageInterface> _mockStore;
        private TestQueue _testQueue;
        private string _topicName;

        public TestEventTopicSender(String topicName, TestQueue testQueue)
        {
            _topicName = topicName;
            _testQueue = testQueue;
            
        }

        public void AddMessage(QueueMessageInterface message)
        {
            _testQueue.AddMessage(message);
            
        }

        public TestQueue GetStore()
        {
            return _testQueue;
        }
    }

    [Serializable]
    public class TestEvent : EventInterface
    {
        public DateTimeOffset TimeStamp { get; set; }
        public String TestMessage { get; set; }
        public string MessageId { get; set; }
    }

    [Serializable]
    public class TestEvent2 : EventInterface
    {
        public DateTimeOffset TimeStamp { get; set; }
        public String TestMessage { get; set; }
        public string MessageId { get; set; }
    }

    [Serializable]
    public class TestGenericEvent<type> : EventInterface
    {
        public DateTimeOffset TimeStamp { get; set; }
        public string MessageId { get; set; }
        public type genric { get; set; }
    }

    public class TestService : HandleEventInterface<TestEvent>
    {
        private Action<string, EventInterface> _queueActionCallback;

        public TestService(Action<string, EventInterface> queueActionCallback)
        {
            _queueActionCallback = queueActionCallback;
        }

        public bool Handle(TestEvent @event)
        {
            _queueActionCallback(@event.MessageId, @event);
            return true;
        }
    }

    public class TestGenericIndexService :
        HandleEventInterface<TestGenericEvent<DateTime>>
        , HandleEventInterface<TestGenericEvent<String>>
        , HandleEventInterface<TestGenericEvent<Int32>>
        , HandleEventInterface<TestEvent2>
    {

        private Action<string, EventInterface> _queueActionCallback;

        public TestGenericIndexService(Action<string, EventInterface> queueActionCallback)
        {
            _queueActionCallback = queueActionCallback;
        }

        public bool Handle(TestGenericEvent<DateTime> @event)
        {
            _queueActionCallback(@event.MessageId, @event);
            return true;
        }

        public bool Handle(TestGenericEvent<string> @event)
        {
            _queueActionCallback(@event.MessageId, @event);
            return true;
        }

        public bool Handle(TestGenericEvent<int> @event)
        {
            _queueActionCallback(@event.MessageId, @event);
            return true;
        }

        public bool Handle(TestEvent2 @event)
        {
            _queueActionCallback(@event.MessageId, @event);
            return true;
        }
    }
}
