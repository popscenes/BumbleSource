using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;
using WebSite.Application.Azure.Binding;
using WebSite.Application.Azure.Content;
using WebSite.Application.Binding;
using WebSite.Application.Command;
using WebSite.Application.Content;
using WebSite.Azure.Common.Environment;
using WebSite.Infrastructure.Command;
using BlobProperties = WebSite.Application.Content.BlobProperties;

namespace WebSite.Application.Azure.Tests
{

    [TestFixture]
    public class AzureCloudBlobStorageTests
    {
        StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [Row("dev")] 
        [Row("real")]
        public AzureCloudBlobStorageTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        } 

        [FixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Get<CloudBlobClient>().GetContainerReference("blobstoragettest").CreateIfNotExist();

            Kernel.Rebind<BlobStorageInterface>().ToMethod(
                ctx => 
                    new AzureCloudBlobStorage(ctx.Kernel.Get<CloudBlobClient>().GetContainerReference("blobstoragettest")));


            Kernel.Rebind<CommandBusInterface>().ToMethod(
                ctx =>
                    ctx.Kernel.Get<CommandQueueFactoryInterface>()
                    .GetCommandBusForEndpoint("commandqueuetest")
            )
            .WhenTargetHas<WorkerCommandBusAttribute>();
            
        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {
            ClearBlobStorage();
            Kernel.Get<CommandQueueFactoryInterface>()
                    .Delete("commandqueuetest");

            AzureEnv.UseRealStorage = false;
        }

        private void ClearBlobStorage()
        {
            
            var testCont = Kernel.Get<CloudBlobClient>().GetContainerReference("blobstoragettest");
            testCont.CreateIfNotExist();
            var blobs = testCont.ListBlobs();
            foreach (var iListBlobItem in blobs)
            {
                var blob = iListBlobItem as CloudBlob;
                if (blob != null) blob.DeleteIfExists();
            }
        }

        [Serializable]
        private class TestBlobStorageCommand : CommandInterface
        {
            #region Implementation of CommandInterface

            public string CommandId { get; set; }

            #endregion

            public byte[] Message { get; set; }
        }

        private string AzureCloudBlobTestStore()
        {
            var data = new byte[1024];
            data[0] = 0; data[1] = 1; data[2] = 2;
            return StoreABlob(data);
        }

        [Test]
        public void AzureCloudBlobTestStoreGetAndDeleteBlob()
        {
            var data = new byte[300];
            data[0] = 0; data[1] = 1; data[2] = 2;
            var id = StoreABlob(data);

            var commandQueueStorageConatiner = Kernel.Get<BlobStorageInterface>();
            Assert.IsInstanceOfType<AzureCloudBlobStorage>(commandQueueStorageConatiner);

            var retrievedData = commandQueueStorageConatiner.GetBlob(id);

            Assert.IsTrue(data.Length == retrievedData.Length);
            for (var i = 0; i < 10; i++)
                Assert.AreEqual(data[i], retrievedData[i]);

            commandQueueStorageConatiner.DeleteBlob(id);

            retrievedData = commandQueueStorageConatiner.GetBlob(id);
            Assert.IsNull(retrievedData);

        }

        [Test]
        public void AzureCloudBlobStoreTestGetByStream()
        {
            var data = new byte[300];
            data[0] = 0; data[1] = 1; data[2] = 2;
            var id = StoreABlob(data);

            var commandQueueStorageConatiner = Kernel.Get<BlobStorageInterface>();
            Assert.IsInstanceOfType<AzureCloudBlobStorage>(commandQueueStorageConatiner);

            using (var memoryStream = new MemoryStream())
            {
                commandQueueStorageConatiner.GetToStream(id, memoryStream);
                var retrievedData = memoryStream.ToArray();

                Assert.IsTrue(data.Length == retrievedData.Length);
                for (var i = 0; i < 10; i++)
                    Assert.AreEqual(data[i], retrievedData[i]);
            }

            commandQueueStorageConatiner.DeleteBlob(id);
        }

        [Test]
        public void TestAzureCloudBlobGetBlobOnNonExistentBlobShouldReturnNull()
        {
            var commandQueueStorageConatiner = Kernel.Get<BlobStorageInterface>();
            var retrievedData = commandQueueStorageConatiner.GetBlob(Guid.NewGuid().ToString());
            Assert.IsNull(retrievedData);
        }

        [Test]
        public void TestAzureCloudBlobGetBlobWidthEmptyOrNullIdShouldReturnNull()
        {
            var commandQueueStorageConatiner = Kernel.Get<BlobStorageInterface>();
            var retrievedData = commandQueueStorageConatiner.GetBlob("");
            Assert.IsNull(retrievedData);

            retrievedData = commandQueueStorageConatiner.GetBlob(null);
            Assert.IsNull(retrievedData);
        }

        private string AzureCloudBlobSetBlobPropertiesDuringCreate()
        {
            var data = new byte[300];
            data[0] = 0; data[1] = 1; data[2] = 2;

            var blobProperties = new BlobProperties()
                                     {
                                         ContentTyp = "image/jpeg",
                                         MetaData = new NameValueCollection(){{"test", "value"}}
                                     };
            var id = StoreABlob(data, blobProperties);
            return id;
        }

        [Test]
        public void AzureCloudBlobSetBlobPropertiesAfterCreate()
        {
            var data = new byte[300];
            data[0] = 0; data[1] = 1; data[2] = 2;
            var id = StoreABlob(data);

            var blobProperties = new BlobProperties()
            {
                ContentTyp = "image/jpeg",
                MetaData = new NameValueCollection() { { "test", "value" } }
            };
            var storage = Kernel.Get<BlobStorageInterface>();
            storage.SetBlobProperties(id, blobProperties);

            AssertGetProperties(id);
        }

        [Test]
        public void AzureCloudBlobGetBlobProperties()
        {
            var id = AzureCloudBlobSetBlobPropertiesDuringCreate();
            AssertGetProperties(id);
        }

        [Test]
        public void AzureCloudBlobGetUri()
        {
            var id = AzureCloudBlobTestStore();
            var storage = Kernel.Get<BlobStorageInterface>();
            var uri = storage.GetBlobUri(id);
            Assert.IsNotNull(uri);
            Assert.Contains(uri.ToString(), id);
        }

        [Test]
        public void AzureCloudBlobExistsReturnsTrue()
        {
            var id = AzureCloudBlobTestStore();
            var storage = Kernel.Get<BlobStorageInterface>();
            Assert.IsTrue(storage.Exists(id));
        }

        [Test]
        public void AzureCloudBlobExistsReturnsFalse()
        {
            var id = Guid.NewGuid().ToString();
            var storage = Kernel.Get<BlobStorageInterface>();
            Assert.IsFalse(storage.Exists(id));
        }


        [Test]
        public void AzureCloudBlobTestUseInsideDataBusCommandSerializer()
        {
            ClearBlobStorage();

            var data = new byte[1024  * 70];
            data[0] = 0; data[1] = 1; data[2] = 2;
            var largeCommand = new TestBlobStorageCommand() { CommandId = Guid.NewGuid().ToString(), Message = data };

            var commandQueueStorageConatiner = Kernel.Get<BlobStorageInterface>();
            var cmdSerial = new DataBusCommandSerializer(commandQueueStorageConatiner);
            byte[] result = cmdSerial.ToByteArray(largeCommand);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
            Assert.IsTrue(result.Length < data.Length);//large command redirects to storage


            Assert.IsInstanceOfType<AzureCloudBlobStorage>(commandQueueStorageConatiner);
            Assert.IsTrue(commandQueueStorageConatiner.BlobCount == 1); 

            cmdSerial.ReleaseCommand(largeCommand);

            Assert.IsTrue(commandQueueStorageConatiner.BlobCount == 0); 
        }

        private void AssertGetProperties(string id)
        {
            var blobProperties = new BlobProperties()
            {
                ContentTyp = "image/jpeg",
                MetaData = new NameValueCollection() { { "test", "value" } }
            };
            var storage = Kernel.Get<BlobStorageInterface>();
            var ret = storage.GetBlobProperties(id);
            Assert.AreEqual(blobProperties.ContentTyp, ret.ContentTyp);
            Assert.AreEqual(blobProperties.MetaData["test"], ret.MetaData["test"]);
        }

        private string StoreABlob(byte[] data, BlobProperties properties = null)
        {
            var commandQueueStorageConatiner = Kernel.Get<BlobStorageInterface>();
            Assert.IsInstanceOfType<AzureCloudBlobStorage>(commandQueueStorageConatiner);
            var id = Guid.NewGuid().ToString();
            Assert.IsTrue(commandQueueStorageConatiner.SetBlob(id, data, properties));
            return id;
        }
    }
}
