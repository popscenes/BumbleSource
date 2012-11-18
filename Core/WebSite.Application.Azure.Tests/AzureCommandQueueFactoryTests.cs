using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using NUnit.Framework;
using Ninject;
using Website.Application.Azure.Command;
using Website.Application.Command;
using Website.Infrastructure.Util;

namespace Website.Application.Azure.Tests
{
    [TestFixture]
    public class AzureCommandQueueFactoryTests
    {
        StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [Test]
        public void TestAzureAzureCommandQueueFactoryCreatesQueues()
        {
            var fact = Kernel.Get<CommandQueueFactoryInterface>();

            Assert.That(fact, Is.InstanceOf<AzureCommandQueueFactory>());

            //just test the hashworks
            var newid = Guid.NewGuid().ToString();
            fact.GetCommandBusForEndpoint(CryptoUtil.CalculateHash(newid));

            var cloudQueueClient = Kernel.Get<CloudQueueClient>();
            var cloudBlobClient = Kernel.Get<CloudBlobClient>();

            var queues = cloudQueueClient.ListQueues().ToList();
            var containers = cloudBlobClient.ListContainers().ToList();

            Assert.IsTrue(queues.Any(q => q.Name == CryptoUtil.CalculateHash(newid)));
            Assert.IsTrue(containers.Any(q => q.Name == CryptoUtil.CalculateHash(newid)));

            cloudQueueClient.GetQueueReference(CryptoUtil.CalculateHash(newid)).Delete();
            cloudBlobClient.GetContainerReference(CryptoUtil.CalculateHash(newid)).Delete();

            queues = cloudQueueClient.ListQueues().ToList();
            containers = cloudBlobClient.ListContainers().ToList();
            Assert.IsFalse(queues.Any(q => q.Name == CryptoUtil.CalculateHash(newid)));
            Assert.IsFalse(containers.Any(q => q.Name == CryptoUtil.CalculateHash(newid)));

            //now test a guid works
            newid = Guid.NewGuid().ToString();
            fact.GetCommandBusForEndpoint(newid);

            queues = cloudQueueClient.ListQueues().ToList();
            containers = cloudBlobClient.ListContainers().ToList();

            Assert.IsTrue(queues.Any(q => q.Name == newid));
            Assert.IsTrue(containers.Any(q => q.Name == newid));


            cloudQueueClient.GetQueueReference(newid).Delete();
            cloudBlobClient.GetContainerReference(newid).Delete();

        }
    }
}
