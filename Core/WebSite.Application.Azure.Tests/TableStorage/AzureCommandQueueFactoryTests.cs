using System;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using NUnit.Framework;
using Ninject;
using Website.Application.Azure.Command;
using Website.Application.Messaging;
using Website.Application.Queue;
using Website.Infrastructure.Util;

namespace Website.Application.Azure.Tests.TableStorage
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
            var fact = Kernel.Get<MessageQueueFactoryInterface>();

            Assert.That(fact, Is.InstanceOf<AzureMessageQueueFactory>());

            //just test the hashworks
            var newid = Guid.NewGuid().ToString();
            fact.GetMessageBusForEndpoint(CryptoUtil.CalculateHash(newid));

            var queueFactory = Kernel.Get<QueueFactoryInterface>();
            var cloudBlobClient = Kernel.Get<CloudBlobClient>();

            var containers = cloudBlobClient.ListContainers().ToList();

            Assert.IsTrue(queueFactory.QueueExists(CryptoUtil.CalculateHash(newid)));
            Assert.IsTrue(containers.Any(q => q.Name == CryptoUtil.CalculateHash(newid)));

            queueFactory.DeleteQueue(CryptoUtil.CalculateHash(newid));
            cloudBlobClient.GetContainerReference(CryptoUtil.CalculateHash(newid)).Delete();

            containers = cloudBlobClient.ListContainers().ToList();
            Assert.IsFalse(queueFactory.QueueExists(CryptoUtil.CalculateHash(newid)));
            Assert.IsFalse(containers.Any(q => q.Name == CryptoUtil.CalculateHash(newid)));

            //now test a guid works
            newid = Guid.NewGuid().ToString();
            fact.GetMessageBusForEndpoint(newid);

            containers = cloudBlobClient.ListContainers().ToList();

            Assert.IsTrue(queueFactory.QueueExists(newid));
            Assert.IsTrue(containers.Any(q => q.Name == newid));

            queueFactory.DeleteQueue(newid);
            cloudBlobClient.GetContainerReference(newid).Delete();

        }
    }
}
