using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
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
using Website.Application.Azure.Command;
using Website.Application.Azure.Queue;
using Website.Application.Azure.ServiceBus;
using Website.Application.Messaging;
using Website.Application.Tests.Mocks;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Types;
using System.Linq;

namespace Website.Application.Azure.Tests.ServiceBus
{
    
    [TestFixture]
    class AzureServicerBusTopicSubscriptionTests
    {

        protected ConcurrentDictionary<string, EventInterface> testHandlerCalls = new ConcurrentDictionary<string, EventInterface>();
            
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
            Kernel.Bind<MessageQueueFactoryInterface>().To<TestMessageQueueFactory>().InThreadScope();

            Kernel.Bind<EventTopicSenderFactoryInterface>().To<AzureEventTopicSenderFactory>().InThreadScope();
            

            Kernel.Bind<CloudBlobContainer>().ToMethod(
                ctx => ctx.Kernel.Get<CloudBlobClient>().GetContainerReference("topicqueuetest"))
                .WithMetadata("topicqueue", true);

            Kernel.Bind<NamespaceManager>().ToMethod(
                ctx => NamespaceManager.CreateFromConnectionString(ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"]));

            Kernel.Bind<MessagingFactory>().ToMethod(
                ctx => MessagingFactory.CreateFromConnectionString(ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"]));

            Kernel.Bind<TestService>().ToMethod(
                ctx => new TestService((messageId, @event) => testHandlerCalls.TryAdd(messageId, @event)));

            Kernel.Bind<TestGenericIndexService>().ToMethod(
                ctx => new TestGenericIndexService((messageId, @event) => testHandlerCalls.TryAdd(messageId, @event)));

            testHandlerCalls.Clear();
        }

        [Test]
        public void TopicBusFactoryCreatesAzureMessageTopicBus()
        {
            var topicBusFactory = Kernel.Get<AzureMessageQueueFactory>();
            var topicBus = topicBusFactory.GetTopicBus("Test");

            //atm just check we return a TopicBus
            Assert.IsTrue(topicBus != null);
        }

        

        [Test]
        public void AzureMessageTopicBusSendMessageTest()
        {
            var topicBusFactory = Kernel.Get<TestMessageQueueFactory>();

            var topicBus = topicBusFactory.GetTopicBus("Test");
            topicBus.Send(new TestEvent() {TimeStamp = DateTime.Now, TestMessage = "testing motha fucker"});

            var queuefactory = Kernel.Get<TestMessageQueueFactory>();
            var queue =  queuefactory.GetTopicSender("Test") as TestEventTopicSender;

            Assert.AreEqual(queue.GetStore().ApproximateMessageCount, 1);
        }

        [Test]
        public void AzureMessageEventTopicSenderFactoryTest()
        {

            var namespaceManager = Kernel.Get<NamespaceManager>();
            var messageFactory = Kernel.Get<MessagingFactory>();

            namespaceManager.DeleteTopic("Test");

            var eventTopicFactory = Kernel.Get<AzureEventTopicSenderFactory>();

            var topicSender = eventTopicFactory.GetTopicSender(typeof (TestEvent).Name.Replace("Event", ""));

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
            List<SubscriptionDetails> subdetails = azureSubscriptionUtils.GetHandlerSubscriptionsFromAssembly(Assembly.GetAssembly(typeof(TestEvent)));

            Assert.AreEqual(subdetails.Count, 3);

            Assert.AreEqual(subdetails[0].Topic, "Test");
            Assert.AreEqual(subdetails[0].Subscription, "TestService");

           
            Assert.AreEqual(subdetails[1].Topic, "Test2");
            Assert.AreEqual(subdetails[1].Subscription, "TestGenericIndexService");

            Assert.AreEqual(subdetails[2].Topic, "TestGeneric");
            Assert.AreEqual(subdetails[2].Subscription, "TestGenericIndexService");
        }

        [Test]
        public void SubscriptionFactoryCreatesAzureMessageSubscriptionProcesor()
        {
            var subscriptionFactory = Kernel.Get<TestMessageQueueFactory>();

            var azureSubscriptionUtils = new AzureSubscriptionUtils();
            List<SubscriptionDetails> subdetails = azureSubscriptionUtils.GetHandlerSubscriptionsFromAssembly(Assembly.GetAssembly(typeof(TestEvent)));
            var subscriptionProcessor = subscriptionFactory.GetProcessorForSubscriptionEndpoint(subdetails[0]);

            //atm just check we return a subscriptionprocessor
            Assert.IsTrue(subscriptionProcessor != null);
        }

        [Test]
        public void FullPubSubTests()
        {
            var azureSubscriptionUtils = new AzureSubscriptionUtils();
            List<SubscriptionDetails> subdetails = azureSubscriptionUtils.GetHandlerSubscriptionsFromAssembly(Assembly.GetAssembly(typeof(TestEvent)));

            var topicBusFactory = Kernel.Get<AzureMessageQueueFactory>();

            var topics = subdetails.Select(_ => _.Topic).Distinct();
            var topicBusDict = topics.ToDictionary(topic => topic, topicBusFactory.GetTopicBus);

            var queuePeocessorList = subdetails.Select(topicBusFactory.GetProcessorForSubscriptionEndpoint).ToList();

            
        }
    }
}
