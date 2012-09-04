using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Syntax;
using Website.Application.Domain.Publish;
using Website.Application.Publish;
using Website.Infrastructure.Command;
using Website.Domain.Claims;
using Website.Domain.Service;
using Website.Mocks.Domain.Data;

namespace Website.Application.Domain.Tests.Claims
{
    [TestFixture]
    public class PublicationServiceTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        public static void FixtureSetUp(MoqMockingKernel kernel)
        {
            kernel.Bind<PublicationServiceInterface>().To<PublicationService>();
        }

        [FixtureSetUp]
        public void FixtureSetUp()
        {
            FixtureSetUp(Kernel);          
        }

        public static void FixtureTearDown(MoqMockingKernel kernel)
        {
            kernel.Unbind<PublicationServiceInterface>();
        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {
            FixtureTearDown(Kernel);
        }

        [Test]
        public void ClaimPublicationServiceSendsPublishCommand()
        {
            Claim pubclaim = null;
            var mockbus = new Moq.Mock<CommandBusInterface>();
            mockbus.Setup(bus => bus.Send(It.IsAny<PublishCommand>()))
                .Returns<PublishCommand>(command => 
                    pubclaim = command.PublishObject as Claim);
            
            Kernel.Bind<CommandBusInterface>()
                .ToConstant(mockbus.Object);

            
            var pubservice = Kernel.Get<PublicationServiceInterface>();
            var claim = new Claim()
                         {
                             AggregateId = Guid.NewGuid().ToString(),
                             BrowserId = Guid.NewGuid().ToString()
                         };
            pubservice.Publish(claim);

            Kernel.Unbind<CommandBusInterface>();


            Assert.IsNotNull(pubclaim);
            ClaimTestData.Equals(claim, pubclaim);
        }
    }
}
