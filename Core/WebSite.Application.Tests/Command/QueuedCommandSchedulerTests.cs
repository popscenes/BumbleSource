using System;
using System.Diagnostics;
using System.Threading;
using Moq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Messaging;
using Website.Application.Tests.Mocks;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Types;
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



        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Bind<MessageQueueFactoryInterface>()
                .ToMethod( ctx => 
                    new TestMessageQueueFactory(Kernel)
                )             
                .InSingletonScope()
                .WithMetadata("queuedmessageprocessortests", true);

            Kernel.Bind<QueuedMessageBus>().ToMethod(
                ctx => ctx.Kernel.Get<MessageQueueFactoryInterface>()
                           .GetMessageBusForEndpoint("queuedmessageprocessortests") as QueuedMessageBus)
                .WithMetadata("queuedmessageprocessortests", true);

            Kernel.Bind<QueuedMessageProcessor>().ToMethod(                
                ctx => ctx.Kernel.Get<MessageQueueFactoryInterface>()
                        .GetProcessorForEndpoint("queuedmessageprocessortests"))
                .WithMetadata("queuedmessageprocessortests", true);
        }

        private TestMessageQueueFactory GetCommandBusFactory()
        {
            return Kernel.Get<MessageQueueFactoryInterface>
                       (ctx => ctx.Has("queuedmessageprocessortests")) as TestMessageQueueFactory;
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<MessageQueueFactoryInterface>();
            Kernel.Unbind<QueuedMessageBus>();
            Kernel.Unbind<QueuedMessageProcessor>();
        }

        [Serializable]
        public class TestCommand : CommandInterface
        {
            public string CommandData { get; set; }
            public string MessageId { get; set; }
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
            var cmdHandler = Kernel.GetMock<MessageHandlerInterface<TestCommand>>();
            cmdHandler.Setup(ch => ch.Handle(It.IsAny<TestCommand>()))
                .Returns<TestCommand>(tc =>
                                           {
                                               Assert.AreEqual(tc.CommandData, "BlahBlahBlah"); 
                                               commandCount++;
                                               return true;
                                           });

            Kernel.Bind<MessageHandlerInterface<TestCommand>>().ToConstant(cmdHandler.Object).InSingletonScope();

            //Add some commands to the queue
            var bus = Kernel.Get<QueuedMessageBus>(ctx => ctx.Has("queuedmessageprocessortests"));
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

            var processor = Kernel.Get<QueuedMessageProcessor>(ctx => ctx.Has("queuedmessageprocessortests"));

            watch.Start();
            processor.Run(cancellationTokenSource.Token);

            Assert.AreEqual(commandCount, deleteCount);
            Assert.AreEqual(commandsReturned, deleteCount);
            Assert.AreEqual(5, deleteCount);

            Kernel.Unbind<MessageHandlerInterface<TestCommand>>();

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
                        Assert.That(lastElapsedMillis, Is.GreaterThanOrEqualTo(lower));
                        Assert.That(lastElapsedMillis, Is.LessThanOrEqualTo(upper));

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

            var processor = Kernel.Get<QueuedMessageProcessor>(ctx => ctx.Has("queuedmessageprocessortests"));

            watch.Start();
            processor.Run(cancellationTokenSource.Token);

            //Assert that it topped out at 1000
            Assert.That(lastElapsedMillis, Is.GreaterThanOrEqualTo(1000));
            Assert.That(lastElapsedMillis, Is.LessThanOrEqualTo(1200));
        }
    }
}
