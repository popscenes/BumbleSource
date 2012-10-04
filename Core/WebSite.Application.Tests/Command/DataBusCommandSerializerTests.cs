using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Command;
using Website.Application.Content;
using Website.Infrastructure.Command;

namespace Website.Application.Tests.Command
{
    [TestFixture]
    public class DataBusCommandSerializerTests
    {
        [Serializable]
        private class DataBusTestCommand : CommandInterface
        {
            public string CommandId { get; set; }
            public byte[] Data { get; set; }
        }

        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<QueueInterface>();
            Kernel.Unbind<QueueMessageInterface>();
            Kernel.Unbind<CommandSerializerInterface>();
            Kernel.Unbind<CommandHandlerInterface<QueuedCommandSchedulerTests.TestCommand>>();
            Kernel.Unbind<MessageFactoryInterface>();
            Kernel.Unbind<BlobStorageInterface>();
        }

        [Test]
        public void DataBusCommandSerializerTestLargeCommandSize()
        {
            var storage = new Dictionary<string, byte[]>();
            var mockstorage = Kernel.GetMock<BlobStorageInterface>();
            mockstorage.Setup(bs=>bs.GetBlob(It.IsAny<string>()))
                .Returns<string>(s =>
                                     {
                                         byte[] ret = null;
                                         storage.TryGetValue(s, out ret);
                                         return ret;
                                     });
            mockstorage.Setup(bs => bs.SetBlob(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<BlobProperties>()))
                .Callback<string, byte[], BlobProperties>((id, d, b) => storage.Add(id, d));

            mockstorage.Setup(bs => bs.DeleteBlob(It.IsAny<string>()))
                .Returns<string>(storage.Remove);

            Kernel.Bind<BlobStorageInterface>().ToConstant(mockstorage.Object).InSingletonScope();

            var data = new byte[1024*1024*5];
            data[0] = 0;data[1] = 1;data[2] = 2;

            var largedatacommand = new DataBusTestCommand() { Data = data, CommandId = Guid.NewGuid().ToString()};
            var cmdSerializer = Kernel.Get<DataBusCommandSerializer>();
            var serializedMessage = cmdSerializer.ToByteArray(largedatacommand);

            //Assert that the data is larger than the seriaised message as the message
            //is just a redirect to blob storage.
            Assert.IsTrue(data.Length > serializedMessage.Length);
            Assert.That(storage.Count, Is.EqualTo(1));

            var retrievedmsg = cmdSerializer.FromByteArray<DataBusTestCommand>(serializedMessage);

            Assert.IsTrue(data.Length == retrievedmsg.Data.Length);
            for (var i = 0; i < 10; i++)
                Assert.AreEqual(data[i], retrievedmsg.Data[i]);

            //Assert that the blob storage is clear after the command released
            Assert.That(storage.Count, Is.EqualTo(1));
            cmdSerializer.ReleaseCommand(retrievedmsg);
            Assert.That(storage.Count, Is.EqualTo(0));

            Kernel.Unbind<BlobStorageInterface>();
        }

        [Test]
        public void DataBusCommandSerializerTestSmallCommandSize()
        {
            var storage = new Dictionary<string, byte[]>();
            var mockstorage = Kernel.GetMock<BlobStorageInterface>();
            mockstorage.Setup(bs => bs.GetBlob(It.IsAny<string>()))
                .Returns<string>(s =>
                {
                    byte[] ret = null;
                    storage.TryGetValue(s, out ret);
                    return ret;
                });
            mockstorage.Setup(bs => bs.SetBlob(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<BlobProperties>()))
                .Callback<string, byte[], BlobProperties>((id, d, b)=> storage.Add(id, d));

            Kernel.Bind<BlobStorageInterface>().ToConstant(mockstorage.Object).InSingletonScope();

            var data = new byte[1024 * 5]; //just 5 k
            data[0] = 0; data[1] = 1; data[2] = 2;

            var largedatacommand = new DataBusTestCommand() { Data = data };
            var cmdSerializer = Kernel.Get<DataBusCommandSerializer>();
            var serializedMessage = cmdSerializer.ToByteArray(largedatacommand);

            //assert that the data is smaller that the serialised object
            Assert.IsTrue(data.Length < serializedMessage.Length);
            Assert.That(storage.Count, Is.EqualTo(0));//shouldn't be anything in storage...

            var retrievedmsg = cmdSerializer.FromByteArray<DataBusTestCommand>(serializedMessage);

            Assert.IsTrue(data.Length == retrievedmsg.Data.Length);
            for (var i = 0; i < 10; i++)
                Assert.AreEqual(data[i], retrievedmsg.Data[i]);

            Kernel.Unbind<BlobStorageInterface>();
        }
    }
}
