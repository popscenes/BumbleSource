using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using WebSite.Application.Command;
using WebSite.Application.Communication;
using WebSite.Application.Tests.Classes;
using WebSite.Application.Tests.Command;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Util;

namespace WebSite.Application.Tests.Communication
{
    [TestFixture]
    public class BroadcastCommunicatorTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        private void Reinit()
        {
            FixtureTearDown();

            Kernel.Bind<CommandQueueFactoryInterface>()
            .ToMethod(ctx =>
                new TestCommandQueueFactory(Kernel)
            )
            .InSingletonScope()
            .WithMetadata("BroadcastCommunicatorTests", true);

            var endPoints = new List<string>();
            var registratorMock = Kernel.GetMock<BroadcastRegistratorInterface>();
            registratorMock.Setup(r => r.GetCurrentEndpoints())
                .Returns(endPoints);

            registratorMock.Setup(r => r.RegisterEndpoint(It.IsAny<string>()))
                .Callback<string>(id => { if (endPoints.Contains(id)) return; endPoints.Add(id); });
            Kernel.Bind<BroadcastRegistratorInterface>().ToConstant(registratorMock.Object);

            Kernel.Bind<BroadcastCommunicatorFactoryInterface>()
                .ToMethod(ctx => new DefaultBroadcastCommunicatorFactory(
                        GetCommandBusFactory(),
                        ctx.Kernel.Get<BroadcastRegistratorInterface>()
                    ));

            _endPoints = new List<string>()
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            };
        }

        [FixtureSetUp]
        public void FixtureSetUp()
        {
            Reinit();
        }


        [FixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<CommandQueueFactoryInterface>();
            Kernel.Unbind<BroadcastRegistratorInterface>();
            Kernel.Unbind<BroadcastCommunicatorFactoryInterface>();
        }

        private TestCommandQueueFactory GetCommandBusFactory()
        {
            return Kernel.Get<CommandQueueFactoryInterface>
                       (ctx => ctx.Has("BroadcastCommunicatorTests")) as TestCommandQueueFactory;
        }

        [Test]
        public void BroadcastCommunicatorQueuedCommandSchedulersProcessBroadcastMessages()
        {          
            BroadcastCommunicatorCommandBusSendsMessageToAllOtherCommunicators();

            var commandCount = 0;
            CancellationTokenSource cancellationTokenSource = null;
            var cmdHandler = Kernel.GetMock<CommandHandlerInterface<QueuedCommandSchedulerTests.TestCommand>>();
            cmdHandler.Setup(ch => ch.Handle(It.IsAny<QueuedCommandSchedulerTests.TestCommand>()))
                .Returns<QueuedCommandSchedulerTests.TestCommand>(tc =>
                {
                    Assert.AreEqual(tc.CommandData, "BlahBlahBlah");
                    commandCount++;
                    cancellationTokenSource.Cancel();
                    return true;
                });

            Kernel.Bind<CommandHandlerInterface<QueuedCommandSchedulerTests.TestCommand>>().ToConstant(cmdHandler.Object).InSingletonScope();

            var factory = Kernel.Get<BroadcastCommunicatorFactoryInterface>();

            foreach (var endPoint in _endPoints.Skip(1))
            {
                var comm = factory.GetCommunicatorForEndpoint(endPoint);
                cancellationTokenSource = new CancellationTokenSource();
                var processor = comm.GetScheduler();
                processor.Run(cancellationTokenSource.Token);
            }


            Assert.AreEqual(2, commandCount);

            Kernel.Unbind<CommandHandlerInterface<QueuedCommandSchedulerTests.TestCommand>>();
        }

        [Test]
        public void BroadcastCommunicatorCommandBusSendsMessageToAllOtherCommunicators()
        {
            Reinit();
            BroadcastCommunicatorRegister();

            var commandBusFactory = GetCommandBusFactory();
            var queues = commandBusFactory.GetTestQueues();
            Assert.Count(3, queues);
            

            var factory = Kernel.Get<BroadcastCommunicatorFactoryInterface>();
            //just grab the first communicator
            var communicator = factory.GetCommunicatorForEndpoint(_endPoints[0]);

            var testCommand = new QueuedCommandSchedulerTests.TestCommand() { CommandData = "BlahBlahBlah" };
            var serializedMsg = SerializeUtil.ToByteArray(testCommand);

            communicator.Send(testCommand);
            foreach (var testQueue in queues.Where(q => q.EndpointName != communicator.Endpoint))
            {
                Assert.Count(1, testQueue.Storage);
                Assert.AreEqual(serializedMsg, testQueue.Storage.ElementAt(0).Bytes);      
            }

            //assert the message isn't sent to the broadcaster
            Assert.Count(0, queues.SingleOrDefault(q => q.EndpointName == communicator.Endpoint).Storage);
           
        }

        [Test]
        public void BroadcastCommunicatorRegister()
        {
            var factory = Kernel.Get<BroadcastCommunicatorFactoryInterface>();

            foreach (var endPoint in _endPoints)
            {
                factory.GetCommunicatorForEndpoint(endPoint);
            }

            
//            var id1 = Guid.NewGuid().ToString();
//            BroadcastCommunicatorInterface communicator1 = factory.GetCommunicatorForEndpoint(id1);
//
//            var id2 = Guid.NewGuid().ToString();
//            BroadcastCommunicatorInterface communicator2 = factory.GetCommunicatorForEndpoint(id2);
//
//            var id3 = Guid.NewGuid().ToString();
//            BroadcastCommunicatorInterface communicator3 = factory.GetCommunicatorForEndpoint(id3);

            var commandBusFactory = GetCommandBusFactory();
            var queues = commandBusFactory.GetTestQueues();
            foreach (var endPoint in _endPoints)
            {
                Assert.Contains(queues.Select(q => q.EndpointName).ToList(), endPoint);
            }

//            Assert.Contains(queues.Select(q => q.EndpointName).ToList(), id1);
//            Assert.Contains(queues.Select(q => q.EndpointName).ToList(), id2);
//            Assert.Contains(queues.Select(q => q.EndpointName).ToList(), id3);

        }

        private static IList<string> _endPoints = new List<string>()
                                                     {
                                                         Guid.NewGuid().ToString(),
                                                         Guid.NewGuid().ToString(),
                                                         Guid.NewGuid().ToString()
                                                     };
    }
}
