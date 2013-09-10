using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using NUnit.Framework;
using Ninject;
using Website.Application.Azure.Queue;
using Website.Application.Content;
using Website.Application.Queue;

namespace Website.Application.Azure.Tests.TableStorage
{
    [TestFixture]
    public class AzureCloudQueueTests
    {
        StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Bind<CloudQueue>().ToMethod(
                ctx => ctx.Kernel.Get<CloudQueueClient>().GetQueueReference("commandqueuetest"))
                .WithMetadata("commandqueue", true);
            Kernel.Bind<CloudBlobContainer>().ToMethod(
                ctx => ctx.Kernel.Get<CloudBlobClient>().GetContainerReference("commandqueuestoragetest"))
                .WithMetadata("commandqueue", true);

            Kernel.Get<CloudBlobContainer>(metadata => metadata.Has("commandqueue")).CreateIfNotExists();
            Kernel.Get<CloudQueue>(metadata => metadata.Has("commandqueue")).CreateIfNotExists();

            Kernel.Bind<QueueInterface>().To<AzureCloudQueue>().WithMetadata("commandqueue", true);

        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<BlobStorageInterface>();   
            Kernel.Unbind<QueueInterface>();

            Kernel.Get<CloudBlobContainer>(metadata => metadata.Has("commandqueue")).Delete();
            Kernel.Get<CloudQueue>(metadata => metadata.Has("commandqueue")).Delete();
            Kernel.Unbind<CloudQueue>();
            Kernel.Unbind<CloudBlobContainer>();
        }


        [Test]
        public void TestAzureCloudQueueAddGetDeleteMessage()
        {

            var azureQueue = Kernel.Get<QueueInterface>(md => md.Has("commandqueue"));
            Assert.That(azureQueue, Is.InstanceOf<AzureCloudQueue>());

            var data = new byte[1024 * 5]; //just 5 k
            data[0] = 0; data[1] = 1; data[2] = 2;
            var msg = new QueueMessage(data);

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
