using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Publish;
using Website.Infrastructure.Command;

namespace Website.Application.Tests.Publish
{
    [TestFixture]
    public class PublishCommandHandlerTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [FixtureSetUp]
        public void FixtureSetUp()
        {
//            Kernel.Bind<CommandHandlerInterface<PublishCommand>>()
//                .To<PublishCommandHandler>();
            Kernel.Bind<CommandBusInterface>().To<DefaultCommandBus>();
        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {
//            Kernel.Unbind<CommandHandlerInterface<PublishCommand>>();
            Kernel.Unbind<CommandBusInterface>();
        }

        [Test]
        public void PublishCommandHandlerPublishesToAllRegisteredPublishServiceInterfaceImplementations()
        {
            bool mock1Called = false;
            var mock1 = new Moq.Mock<PublishServiceInterface<TestPublishClass>>();
            mock1.Setup(service => service.Publish(It.IsAny<TestPublishClass>()))
                .Returns<TestPublishClass>(pub =>
                                               {
                                                   mock1Called = true;
                                                   return true;
                                               });
            mock1.SetupGet(service => service.IsEnabled)
                .Returns(true);

            Kernel.Bind<PublishServiceInterface<TestPublishClass>>()
                .ToConstant(mock1.Object);
             
            bool mock2Called = false;
            var mock2 = new Moq.Mock<PublishServiceInterface<TestPublishClass>>();
            mock2.Setup(service => service.Publish(It.IsAny<TestPublishClass>()))
                .Returns<TestPublishClass>(pub =>
                                               {
                                                   mock2Called = true;
                                                   return true;
                                               });
            mock2.SetupGet(service => service.IsEnabled)
                .Returns(true);

            Kernel.Bind<PublishServiceInterface<TestPublishClass>>()
                .ToConstant(mock2.Object);


            var bus = Kernel.Get<CommandBusInterface>();
            bus.Send(new PublishCommand() {PublishObject = new TestPublishClass {YaMum = "loves it"}});

            Assert.IsTrue(mock1Called);
            Assert.IsTrue(mock2Called);
        }

        public class TestPublishClass
        {
            internal string YaMum { get; set; }
        }
    }
}
