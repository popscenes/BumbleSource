﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.ApplicationCommunication;
using Website.Application.Command;
using Website.Application.Tests.Command;
using Website.Application.Tests.Mocks;
using Website.Infrastructure.Command;
using Website.Infrastructure.Types;
using Website.Infrastructure.Util;

namespace Website.Application.Tests.ApplicationCommunication
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
            var registratorMock = Kernel.GetMock<ApplicationBroadcastCommunicatorRegistrationInterface>();
            registratorMock.Setup(r => r.GetCurrentEndpoints())
                .Returns(endPoints);

            registratorMock.Setup(r => r.RegisterEndpoint(It.IsAny<string>()))
                .Callback<string>(id => { if (endPoints.Contains(id)) return; endPoints.Add(id); });
            Kernel.Bind<ApplicationBroadcastCommunicatorRegistrationInterface>().ToConstant(registratorMock.Object);

            Kernel.Bind<ApplicationBroadcastCommunicatorFactoryInterface>()
                .ToMethod(ctx => new DefaultApplicationBroadcastCommunicatorFactory(
                        GetCommandBusFactory(),
                        ctx.Kernel.Get<ApplicationBroadcastCommunicatorRegistrationInterface>()
                    ));

            _endPoints = new List<string>()
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            };
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Reinit();
        }


        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<CommandQueueFactoryInterface>();
            Kernel.Unbind<ApplicationBroadcastCommunicatorRegistrationInterface>();
            Kernel.Unbind<ApplicationBroadcastCommunicatorFactoryInterface>();
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

            var factory = Kernel.Get<ApplicationBroadcastCommunicatorFactoryInterface>();

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
            Assert.That(queues.Count(), Is.EqualTo(3));
            

            var factory = Kernel.Get<ApplicationBroadcastCommunicatorFactoryInterface>();
            //just grab the first communicator
            var communicator = factory.GetCommunicatorForEndpoint(_endPoints[0]);

            var testCommand = new QueuedCommandSchedulerTests.TestCommand() { CommandData = "BlahBlahBlah" };
            var serializedMsg = SerializeUtil.ToByteArray(testCommand);

            communicator.Send(testCommand);
            foreach (var testQueue in queues.Where(q => q.EndpointName != communicator.Endpoint))
            {
                Assert.That(testQueue.Storage.Count, Is.EqualTo(1));
                Assert.AreEqual(serializedMsg, testQueue.Storage.ElementAt(0).Bytes);      
            }

            //assert the message isn't sent to the broadcaster
            var count = queues.SingleOrDefault(q => q.EndpointName == communicator.Endpoint).Storage.Count;
            Assert.That(count, Is.EqualTo(0));           
        }

        [Test]
        public void BroadcastCommunicatorRegister()
        {
            var factory = Kernel.Get<ApplicationBroadcastCommunicatorFactoryInterface>();

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
                CollectionAssert.Contains(queues.Select(q => q.EndpointName).ToList(), endPoint);
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
