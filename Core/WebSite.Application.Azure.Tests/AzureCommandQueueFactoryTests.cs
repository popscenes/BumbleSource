﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;
using WebSite.Application.Azure.Command;
using WebSite.Application.Command;
using WebSite.Infrastructure.Util;

namespace WebSite.Application.Azure.Tests
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

            Assert.IsInstanceOfType<AzureCommandQueueFactory>(fact);

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
