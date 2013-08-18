using NUnit.Framework;
using Website.Application.Azure.Queue;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Util.Extension;

namespace Website.Application.Azure.Tests.ServiceBus
{
    [TestFixture]
    public class ServiceBusQueueFactoryTests
    {
        [Test]
        public void ServiceBusQueueFactoryCreatesQueue()
        {
            var test = new ServiceBusQueueFactory(Config.Instance, "ServiceBusNamespace",
                System.Environment.MachineName.ToLowerHiphen());

            var testQueue = test.GetQueue("testQueue");
            Assert.That(testQueue, Is.Not.Null);

            Assert.That(test.QueueExists("testQueue"), Is.True);

            test.DeleteQueue("testQueue");

            Assert.That(test.QueueExists("testQueue"), Is.False);


        }
    }
}
