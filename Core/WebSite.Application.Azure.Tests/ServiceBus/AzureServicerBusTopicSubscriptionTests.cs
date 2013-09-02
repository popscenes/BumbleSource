using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage.Blob;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Syntax;
using PostaFlya.DataRepository.Search.Event;
using Website.Application.Azure.Content;
using Website.Application.Azure.Queue;
using Website.Application.Messaging;
using Website.Application.Queue;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Types;

namespace Website.Application.Azure.Tests.ServiceBus
{
    
    [TestFixture]
    class AzureServicerBusTopicSubscriptionTests
    {

        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<EventTopicSenderFactoryInterface>();
            Kernel.Unbind<CloudBlobContainer>();
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Bind<EventTopicSenderFactoryInterface>().To<TestEventTopicSenderFactory>().InThreadScope();
            

            Kernel.Bind<CloudBlobContainer>().ToMethod(
                ctx => ctx.Kernel.Get<CloudBlobClient>().GetContainerReference("topicqueuetest"))
                .WithMetadata("topicqueue", true);

            Kernel.Bind<NamespaceManager>().ToMethod(
                ctx => NamespaceManager.CreateFromConnectionString(ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"]));

            Kernel.Bind<MessagingFactory>().ToMethod(
                ctx => MessagingFactory.CreateFromConnectionString(ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"]));
        }

        [Test]
        public void TopicBusFactoryCreatesAzureMessageTopicBus()
        {
            var topicBusFactory = Kernel.Get<AzureMessageEventTopicBusFactory>();
            var topicBus = topicBusFactory.GetTopicBus("Test");

            //atm just check we return a TopicBus
            Assert.IsTrue(topicBus != null);
        }

        

        [Test]
        public void AzureMessageTopicBusSendMessageTest()
        {
            var topicBusFactory = Kernel.Get<AzureMessageEventTopicBusFactory>();

            var topicBus = topicBusFactory.GetTopicBus("Test");
            topicBus.Send(new TestEvent() {TimeStamp = DateTime.Now, TestMessage = "testing motha fucker"});

            var queuefactory = Kernel.Get<EventTopicSenderFactoryInterface>();
            var queue =  queuefactory.GetTopicSender("Test") as TestEventTopicSender;

            Assert.AreEqual(queue.GetStore().Count, 1);
        }

        [Test]
        public void AzureMessageEventTopicSenderFactoryTest()
        {

            var namespaceManager = Kernel.Get<NamespaceManager>();
            var messageFactory = Kernel.Get<MessagingFactory>();

            var eventTopicFactory = Kernel.Get<AzureEventTopicSenderFactory>();

            var topicSender = eventTopicFactory.GetTopicSender(typeof (TestEvent));

            Assert.IsNotNull(topicSender);
            Assert.IsTrue(namespaceManager.TopicExists("Test"));

            namespaceManager.CreateSubscription("Test", "senderTest");

            var testEvent = new TestEvent() {MessageId = "te1", TestMessage = "fuckin"};

            topicSender.AddMessage(new ServiceBusQueueMessage(SerializeUtil.ToByteArray(testEvent)));

            var subClient = messageFactory.CreateSubscriptionClient("Test", "senderTest");
            var message = subClient.Receive(TimeSpan.FromSeconds(10));

            Assert.IsNotNull(message);

            testEvent = SerializeUtil.FromByteArray(message.GetBody<byte[]>()) as TestEvent;

            Assert.AreEqual(testEvent.TestMessage, "fuckin");

            namespaceManager.DeleteTopic("Test");
        }

        [Test]
        public void GetSubscriptionNamesFromAssembly()
        {
            var azureSubscriptionUtils = new AzureSubscriptionUtils();
            List<AzureSubscriptionDetails> subdetails = azureSubscriptionUtils.GetHandlerSubscriptionsFromAssembly(Assembly.GetAssembly(typeof(TestEvent)));

            Assert.AreEqual(subdetails.Count, 3);

            Assert.AreEqual(subdetails[0].Topic, "Test");
            Assert.AreEqual(subdetails[0].Subscription, "TestIndexService");

           
            Assert.AreEqual(subdetails[1].Topic, "Test2");
            Assert.AreEqual(subdetails[1].Subscription, "TestGenericIndexService");

            Assert.AreEqual(subdetails[2].Topic, "TestGeneric");
            Assert.AreEqual(subdetails[2].Subscription, "TestGenericIndexService");
        }

        [Test]
        public void SubscriptionFactoryCreatesAzureMessageSubscriptionProcesor()
        {
            var subscriptionFactory = Kernel.Get<AzureEventSubscriptionProcessorFactory>();

            var azureSubscriptionUtils = new AzureSubscriptionUtils();
            List<AzureSubscriptionDetails> subdetails = azureSubscriptionUtils.GetHandlerSubscriptionsFromAssembly(Assembly.GetAssembly(typeof(TestEvent)));
            var subscriptionProcessor = subscriptionFactory.GetSubscriptionProcessor(subdetails[0]);

            //atm just check we return a subscriptionprocessor
            Assert.IsTrue(subscriptionProcessor != null);
        }

        [Test]
        public void CreateFilteredSubscriptionsFromConfig()
        {

        }
    }

    public class AzureSubscriptionDetails
    {
        public String Topic { get; set; }
        public String Subscription { get; set; }
        public Type HandlerType { get; set; }
    }

    public class AzureSubscriptionUtils
    {
        public List<AzureSubscriptionDetails> GetHandlerSubscriptionsFromAssembly(Assembly assembly)
        {
            var eventInterface = typeof(EventInterface);
            var eventsTypes = TypeUtil.GetAllSubTypesFrom(eventInterface, assembly);
            var listOfSubs = new List<AzureSubscriptionDetails>();

            foreach (var type in eventsTypes)
            {
                if (type.IsGenericType)
                {
                    var handlers =
                        assembly.DefinedTypes.Where(
                            dt =>
                            dt.ImplementedInterfaces.Any(_ => _.NameLike(typeof(HandleEventInterface<>).Name) && _.GenericTypeArguments.Any(gt => gt.NameLike(type.Name))));


                    listOfSubs.AddRange(handlers.Select(handler => new AzureSubscriptionDetails() { Topic = type.Name.Replace("Event", "").Replace("`1", ""), Subscription = handler.Name.Replace("`1", ""), HandlerType = handler}));
                    continue;
                }


                Type handlerType = typeof(HandleEventInterface<>).MakeGenericType(type);
                var handlerTypes = TypeUtil.GetExpandedImplementorsUsing(handlerType, assembly);
                listOfSubs.AddRange(handlerTypes.Select(handler => new AzureSubscriptionDetails() { Topic = type.Name.Replace("Event", ""), Subscription = handler.Name.Replace("`1", ""), HandlerType = handler }));
            }

            return listOfSubs;
        }
    }


    public class AzureEventTopicSenderFactory : EventTopicSenderFactoryInterface
    {
        private readonly NamespaceManager _namespaceManager;
        private readonly MessagingFactory _messagingFactory;

        private const string DefaultSubscription = "nofilter";

        public AzureEventTopicSenderFactory(NamespaceManager namespaceManager, MessagingFactory messagingFactory)
        {
            _namespaceManager = namespaceManager;
            _messagingFactory = messagingFactory;
        }

        public EventTopicSenderInterface GetTopicSender(Type type)
        {
            return GetTopicSender(type.Name.Replace("Event", ""));
        }

        public EventTopicSenderInterface GetTopicSender(string name)
        {
            if (_namespaceManager.TopicExists(name))
            {
                return new AzureEventTopcSender(_messagingFactory.CreateTopicClient(name));
            }

            _namespaceManager.CreateTopic(name);

            return new AzureEventTopcSender(_messagingFactory.CreateTopicClient(name));
        }
    }

    public class AzureEventTopcSender : EventTopicSenderInterface
    {
        private readonly TopicClient _topicClient;

        public AzureEventTopcSender(TopicClient topicClient)
        {
            _topicClient = topicClient;
        }

        public void AddMessage(QueueMessageInterface message)
        {
            var azureMsg = message as ServiceBusQueueMessage ?? new ServiceBusQueueMessage(message.Bytes);
            _topicClient.Send(azureMsg.Message);
        }
    }

    [Serializable]
    public class TestEvent: EventInterface
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
        public String TestMessage { get; set; }
        public string MessageId { get; set; }
        public type genric { get; set; }
    }

    public class TestIndexService :HandleEventInterface<TestEvent>
    {
        public bool Handle(TestEvent @event)
        {
            throw new NotImplementedException();
        }
    }
    public class TestGenericIndexService :
        HandleEventInterface<TestGenericEvent<DateTime>>
        , HandleEventInterface<TestGenericEvent<String>>
        , HandleEventInterface<TestGenericEvent<Int32>>
        , HandleEventInterface<TestEvent2>
    {
        public bool Handle(TestGenericEvent<DateTime> @event)
        {
            throw new NotImplementedException();
        }

        public bool Handle(TestGenericEvent<string> @event)
        {
            throw new NotImplementedException();
        }

        public bool Handle(TestGenericEvent<int> @event)
        {
            throw new NotImplementedException();
        }

        public bool Handle(TestEvent2 @event)
        {
            throw new NotImplementedException();
        }
    }

    public class AzureEventSubscriptionProcessorFactory
    {
        private readonly AzureEventSubscriptionRecieverFactory _recieverFactory;
        private readonly CloudBlobClient _cloudBlobClient;
        private readonly IResolutionRoot _resolver;

        public AzureEventSubscriptionProcessorFactory(AzureEventSubscriptionRecieverFactory recieverFactory, CloudBlobClient cloudBlobClient, IResolutionRoot resolver)
        {
            _recieverFactory = recieverFactory;
            _cloudBlobClient = cloudBlobClient;
            _resolver = resolver;
        }

        public AzureEventSubscriptionProcessor GetSubscriptionProcessor(AzureSubscriptionDetails subscriptionDetails)
        {

            var messageSerializer = GetSerializerForEndpoint(subscriptionDetails.Topic);
            var handler = _resolver.Get(subscriptionDetails.HandlerType) as HandleEventInterface;
            var subscriptionReciever = _recieverFactory.GetSubscriptionReciever(subscriptionDetails.Subscription);

            var azureEventSubscriptionProcessor = new AzureEventSubscriptionProcessor(subscriptionReciever, handler, messageSerializer);
            return azureEventSubscriptionProcessor;
        }

        private MessageSerializerInterface GetSerializerForEndpoint(string topinName)
        {
            var queueStorage = new AzureCloudBlobStorage(_cloudBlobClient.GetContainerReference(topinName));
            queueStorage.CreateIfNotExists();
            return new DataBusMessageSerializer(queueStorage);
        }
    }

    public class AzureEventSubscriptionRecieverFactory : EventSubscriptionRecieverFactory
    {
        public SubscriptionReciever GetSubscriptionReciever(string subscription)
        {
            throw new NotImplementedException();
        }
    }

    public interface SubscriptionReciever
    {
    }

    public interface EventSubscriptionRecieverFactory
    {
        SubscriptionReciever GetSubscriptionReciever(string subscription);
    }

    public class AzureEventSubscriptionProcessor : EventSubscriptionProcessorInterface
    {
        public AzureEventSubscriptionProcessor(SubscriptionReciever subscriptionReciever, HandleEventInterface handler, MessageSerializerInterface messageSerializer)
        {
            throw new NotImplementedException();
        }
    }

    public interface EventSubscriptionProcessorInterface
    {
    }

    public class AzureMessageEventTopicBusFactory
    {
        private readonly EventTopicSenderFactoryInterface _eventTopicSenderFactory;
        private readonly CloudBlobClient _cloudBlobClient;

        public AzureMessageEventTopicBusFactory(EventTopicSenderFactoryInterface eventTopicSenderFactory, CloudBlobClient cloudBlobClient)
        {
            _eventTopicSenderFactory = eventTopicSenderFactory;
            _cloudBlobClient = cloudBlobClient;
        }

        public AzureMessageTopicBus GetTopicBus(string topinName)
        {
            var topicQueue = _eventTopicSenderFactory.GetTopicSender(topinName);
            var serializer = GetSerializerForEndpoint(topinName);
            return new AzureMessageTopicBus(topicQueue, serializer);
        }

        private MessageSerializerInterface GetSerializerForEndpoint(string topinName)
        {
            var queueStorage = new AzureCloudBlobStorage(_cloudBlobClient.GetContainerReference(topinName));
            queueStorage.CreateIfNotExists();
            return new DataBusMessageSerializer(queueStorage);
        }
    }

    
    public interface EventTopicSenderFactoryInterface
    {
        EventTopicSenderInterface GetTopicSender(Type type);
        EventTopicSenderInterface GetTopicSender(string name);
    }

    public interface EventTopicSenderInterface : QueueSenderInterface
    {
     
    }

    public class TestEventTopicSenderFactory: EventTopicSenderFactoryInterface
    {
        private readonly ConcurrentDictionary<string, TestEventTopicSender> _queues = new ConcurrentDictionary<string, TestEventTopicSender>();
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
                _queues[name] =  eventTopicSender;
            }
            return eventTopicSender;
        }
    }

    public class TestEventTopicSender : EventTopicSenderInterface
    {
        private List<QueueMessageInterface> _mockStore;
        private string _topicName;

        public TestEventTopicSender(String topicName)
        {
            _topicName = topicName;
            _mockStore = new List<QueueMessageInterface>();
        }

        public void AddMessage(QueueMessageInterface message)
        {
            _mockStore.Add(message);
        }

        public List<QueueMessageInterface> GetStore()
        {
            return _mockStore;
        }
    }

    public interface TopicMessageInterface : QueueMessageInterface
    {
        
    }

    public class AzureMessageTopicBus : TopicBusInterface
    {
        private readonly EventTopicSenderInterface _eventTopicSender;
        private readonly MessageSerializerInterface _messageSerializer;

        public AzureMessageTopicBus(EventTopicSenderInterface eventTopicSender, MessageSerializerInterface messageSerializer)
        {
            _eventTopicSender = eventTopicSender;
            _messageSerializer = messageSerializer;
        }

        public object Send<EventType>(EventType @event) where EventType : class, EventInterface
        {
            var message = _messageSerializer.ToByteArray(@event);
            var sendmessage = new ServiceBusQueueMessage(message);
            _eventTopicSender.AddMessage(sendmessage);
            return true;
        }
    }

    public interface TopicBusInterface
    {
        object Send<EventType>(EventType @event) where EventType : class, EventInterface;
    }


}
