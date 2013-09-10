using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Messaging;
using Website.Application.Tests.Mocks;
using Website.Infrastructure.Types;
using Website.Infrastructure.Util;

namespace Website.Application.Tests.Command
{
    [TestFixture]
    public class QueueCommandBusTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Bind<MessageQueueFactoryInterface>()
                .ToMethod(ctx =>
                    new TestMessageQueueFactory(Kernel)
                )                
                .InSingletonScope()
                .WithMetadata("queuedcommandbustests", true);

            Kernel.Bind<QueuedMessageBus>().ToMethod(
                ctx => ctx.Kernel.Get<MessageQueueFactoryInterface>()
                           .GetMessageBusForEndpoint("queuedcommandbustests") as QueuedMessageBus)
                .WithMetadata("queuedcommandbustests", true);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<MessageQueueFactoryInterface>();
        }

        private TestMessageQueueFactory GetCommandBusFactory()
        {
            return Kernel.Get<MessageQueueFactoryInterface>
                       (ctx => ctx.Has("queuedcommandbustests")) as TestMessageQueueFactory;
        }


        [Test]
        public void QueueCommandBusAddMessageTest()
        {
            //moq the command for message
            var testCommand = new QueuedMessageProcessorTests.TestCommand() { CommandData = "BlahBlahBlah" };
            var serializedMsg = SerializeUtil.ToByteArray(testCommand);

            var bus = Kernel.Get<QueuedMessageBus>(ctx => ctx.Has("queuedcommandbustests"));
            for (int i = 0; i < 5; i++)
            {
                bus.Send(testCommand);
            }

            var testQueuedCommandBusFactory = GetCommandBusFactory();
            Assert.That(testQueuedCommandBusFactory.GetStorageForTestEndpoint("queuedcommandbustests").Count, Is.EqualTo(5));
            foreach (var queueMessage in testQueuedCommandBusFactory.GetStorageForTestEndpoint("queuedcommandbustests"))
            {
                Assert.AreEqual(serializedMsg, queueMessage.Bytes);
            }
        }
    }
}
