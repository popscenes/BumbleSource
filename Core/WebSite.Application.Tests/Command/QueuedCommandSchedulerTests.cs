using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using MbUnit.Framework;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Command;
using Website.Application.Tests.Classes;
using Website.Infrastructure.Command;
using Website.Infrastructure.Util;

namespace Website.Application.Tests.Command
{
    [TestFixture]
    public class QueuedCommandSchedulerTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }



        [FixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Bind<CommandQueueFactoryInterface>()
                .ToMethod( ctx => 
                    new TestCommandQueueFactory(Kernel)
                )             
                .InSingletonScope()
                .WithMetadata("queuedmessageprocessortests", true);

            Kernel.Bind<QueuedCommandBus>().ToMethod(
                ctx => ctx.Kernel.Get<CommandQueueFactoryInterface>()
                           .GetCommandBusForEndpoint("queuedmessageprocessortests") as QueuedCommandBus)
                .WithMetadata("queuedmessageprocessortests", true);

            Kernel.Bind<QueuedCommandScheduler>().ToMethod(                
                ctx => ctx.Kernel.Get<CommandQueueFactoryInterface>()
                        .GetSchedulerForEndpoint("queuedmessageprocessortests"))
                .WithMetadata("queuedmessageprocessortests", true);
        }

        private TestCommandQueueFactory GetCommandBusFactory()
        {
            return Kernel.Get<CommandQueueFactoryInterface>
                       (ctx => ctx.Has("queuedmessageprocessortests")) as TestCommandQueueFactory;
        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<CommandQueueFactoryInterface>();
            Kernel.Unbind<QueuedCommandBus>();
            Kernel.Unbind<QueuedCommandScheduler>();
        }

        [Serializable]
        public class TestCommand : CommandInterface
        {
            public string CommandData { get; set; }
            public string CommandId { get; set; }
        }

        [Test]
        public void QueueMessageProcessorWaitsForAllWorkInProgressToFinish()
        {      
            var cancellationTokenSource = new CancellationTokenSource();
            var watch = new Stopwatch();

            //create some messages
            var testCommand = new TestCommand() {CommandData = "BlahBlahBlah"};
            var serializedMsg = SerializeUtil.ToByteArray(testCommand);
  
            //setup a command handler
            var commandCount = 0;
            var cmdHandler = Kernel.GetMock<CommandHandlerInterface<TestCommand>>();
            cmdHandler.Setup(ch => ch.Handle(It.IsAny<TestCommand>()))
                .Returns<TestCommand>(tc =>
                                           {
                                               Assert.AreEqual(tc.CommandData, "BlahBlahBlah"); 
                                               commandCount++;
                                               return true;
                                           });

            Kernel.Bind<CommandHandlerInterface<TestCommand>>().ToConstant(cmdHandler.Object).InSingletonScope();

            //Add some commands to the queue
            var bus = Kernel.Get<QueuedCommandBus>(ctx => ctx.Has("queuedmessageprocessortests"));
            for (int i = 0; i < 5; i++)
            {
                bus.Send(testCommand);
            }

           
            var deleteCount = 0;
            var testQueuedCommandBusFactory = GetCommandBusFactory();

            testQueuedCommandBusFactory.AddQueueActionListener(
                (endpoint, method, msg) =>
                {
                    if(method == "DeleteMessage")
                    {
                        deleteCount++;
                        if (deleteCount == 5)
                            cancellationTokenSource.Cancel();
                    }
                    else if (method == "GetMessage")
                    {
                        if (commandCount < 5)
                        {   //to make sure the queue doesn't do any backing off while there are messages
                            //Assert.LessThan(watch.ElapsedMilliseconds, 200);
                            watch.Restart();
                        }
                    }

                });

            var commandsReturned = 0;
            testQueuedCommandBusFactory.AddCmdSerializerListener(
                (method, cmd) =>
                {
                    if (method != "ReleaseCommand") return;
                    commandsReturned++;
                }
                );

            var processor = Kernel.Get<QueuedCommandScheduler>(ctx => ctx.Has("queuedmessageprocessortests"));

            watch.Start();
            processor.Run(cancellationTokenSource.Token);

            Assert.AreEqual(commandCount, deleteCount);
            Assert.AreEqual(commandsReturned, deleteCount);
            Assert.AreEqual(5, deleteCount);

            Kernel.Unbind<CommandHandlerInterface<TestCommand>>();

        }

        [Test]
        public void QueueMessageProcessorShouldBackOffToWaitFor1SecondWhenNotRecievingMessages()
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var watch = new Stopwatch();

            int lower = 0, upper = 200;
            long lastElapsedMillis = 0;
            int count = 0;

            var testQueuedCommandBusFactory = GetCommandBusFactory();
            testQueuedCommandBusFactory.AddQueueActionListener(
                (endpoint, method, msg) =>
                {
                    if (method == "GetMessage")
                    {
                        lastElapsedMillis = watch.ElapsedMilliseconds + 5;//just push it a little
                        Assert.GreaterThanOrEqualTo(lastElapsedMillis, lower);
                        Assert.LessThanOrEqualTo(lastElapsedMillis, upper);
                        if (lower < 1000)
                        {
                            lower += 200;
                            upper += 200;
                        }

                        if (++count == 7)// 0, 200, 400, 600, 800, 1000, 1000
                            cancellationTokenSource.Cancel();
                        watch.Restart();
                    }

                });

            var processor = Kernel.Get<QueuedCommandScheduler>(ctx => ctx.Has("queuedmessageprocessortests"));

            watch.Start();
            processor.Run(cancellationTokenSource.Token);

            //Assert that it topped out at 1000
            Assert.GreaterThanOrEqualTo(lastElapsedMillis, 1000);
            Assert.LessThanOrEqualTo(lastElapsedMillis, 1200);
        }
    }
}
