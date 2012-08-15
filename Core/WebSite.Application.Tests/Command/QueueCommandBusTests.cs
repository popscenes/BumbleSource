using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using WebSite.Application.Command;
using WebSite.Application.Tests.Classes;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Util;

namespace WebSite.Application.Tests.Command
{
    [TestFixture]
    public class QueueCommandBusTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [FixtureSetUp]
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

        [FixtureTearDown]
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
            Assert.Count(5, testQueuedCommandBusFactory.GetStorageForTestEndpoint("queuedcommandbustests"));
            foreach (var queueMessage in testQueuedCommandBusFactory.GetStorageForTestEndpoint("queuedcommandbustests"))
            {
                Assert.AreEqual(serializedMsg, queueMessage.Bytes);
            }
        }
    }
}
