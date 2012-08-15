using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;
using WebSite.Application.Azure.Command;
using WebSite.Application.Azure.Content;
using WebSite.Application.Command;
using WebSite.Application.Content;
using WebSite.Infrastructure.Command;

namespace WebSite.Application.Azure.Tests
{
    [TestFixture]
    public class AzureCloudQueueTests
    {
        StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [FixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Bind<CloudQueue>().ToMethod(
                ctx => ctx.Kernel.Get<CloudQueueClient>().GetQueueReference("commandqueuetest"))
                .WithMetadata("commandqueue", true);
            Kernel.Bind<CloudBlobContainer>().ToMethod(
                ctx => ctx.Kernel.Get<CloudBlobClient>().GetContainerReference("commandqueuestoragetest"))
                .WithMetadata("commandqueue", true);

            Kernel.Get<CloudBlobContainer>(metadata => metadata.Has("commandqueue")).CreateIfNotExist();
            Kernel.Get<CloudQueue>(metadata => metadata.Has("commandqueue")).CreateIfNotExist();

            Kernel.Bind<MessageFactoryInterface>().To<AzureMessageFactory>();
            Kernel.Bind<QueueInterface>().To<AzureCloudQueue>().WithMetadata("commandqueue", true);

        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<BlobStorageInterface>();   
            Kernel.Unbind<QueueInterface>();
            Kernel.Unbind<MessageFactoryInterface>();

            Kernel.Get<CloudBlobContainer>(metadata => metadata.Has("commandqueue")).Delete();
            Kernel.Get<CloudQueue>(metadata => metadata.Has("commandqueue")).Delete();
            Kernel.Unbind<CloudQueue>();
            Kernel.Unbind<CloudBlobContainer>();
        }


        [Test]
        public void TestAzureCloudQueueAddGetDeleteMessage()
        {
            var messageFactory = Kernel.Get<MessageFactoryInterface>();
            Assert.IsInstanceOfType<AzureMessageFactory>(messageFactory);

            var azureQueue = Kernel.Get<QueueInterface>(md => md.Has("commandqueue"));
            Assert.IsInstanceOfType<AzureCloudQueue>(azureQueue);

            var data = new byte[1024 * 5]; //just 5 k
            data[0] = 0; data[1] = 1; data[2] = 2;
            var msg = messageFactory.GetMessageForBytes(data);

            azureQueue.AddMessage(msg);

            var retrievedMsg = azureQueue.GetMessage();

            Assert.IsNotNull(retrievedMsg);
            Assert.IsNotNull(retrievedMsg.Bytes);
            Assert.IsTrue(data.Length == retrievedMsg.Bytes.Length);
            for (var i = 0; i < 10; i++)
                Assert.AreEqual(data[i], retrievedMsg.Bytes[i]);

            var tryAnotherMsg = azureQueue.GetMessage();
            Assert.IsNull(tryAnotherMsg);

            azureQueue.DeleteMessage(retrievedMsg);

        }
    }
}
