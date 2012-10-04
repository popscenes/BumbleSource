using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Command;
using Website.Application.Tests.Classes;
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
            Kernel.Bind<CommandQueueFactoryInterface>()
                .ToMethod(ctx =>
                    new TestCommandQueueFactory(Kernel)
                )                
                .InSingletonScope()
                .WithMetadata("queuedcommandbustests", true);

            Kernel.Bind<QueuedCommandBus>().ToMethod(
                ctx => ctx.Kernel.Get<CommandQueueFactoryInterface>()
                           .GetCommandBusForEndpoint("queuedcommandbustests") as QueuedCommandBus)
                .WithMetadata("queuedcommandbustests", true);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<CommandQueueFactoryInterface>();
        }

        private TestCommandQueueFactory GetCommandBusFactory()
        {
            return Kernel.Get<CommandQueueFactoryInterface>
                       (ctx => ctx.Has("queuedcommandbustests")) as TestCommandQueueFactory;
        }


        [Test]
        public void QueueCommandBusAddMessageTest()
        {
            //moq the command for message
            var testCommand = new QueuedCommandSchedulerTests.TestCommand() { CommandData = "BlahBlahBlah" };
            var serializedMsg = SerializeUtil.ToByteArray(testCommand);

            var bus = Kernel.Get<QueuedCommandBus>(ctx => ctx.Has("queuedcommandbustests"));
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
