using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Ninject.MockingKernel.Moq;
using Website.Application.Domain.TinyUrl;
using Website.Application.Queue;
using Website.Application.Schedule;
using Website.Application.Tests.Mocks;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using Website.Test.Common;

namespace Website.Application.Domain.Tests.TinyUrl
{
    [TestFixture]
    public class TinyUrlGenerationJobActionTests
    {
        MoqMockingKernel Kernel
        {
            get { return Application.Tests.TestFixtureSetup.CurrIocKernel; }
        }

        readonly HashSet<TinyUrlRecordInterface> _store = RepoCoreUtil.GetMockStore<TinyUrlRecordInterface>();
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            RepoCoreUtil.SetupRepo<GenericRepositoryInterface, TinyUrlRecord, TinyUrlRecordInterface, TinyUrlRecordInterface>(_store, Kernel, TinyUrlRecordInterfaceExtensions.CopyFieldsFrom);
            RepoCoreUtil.SetupQueryService<GenericQueryServiceInterface, TinyUrlRecord, TinyUrlRecordInterface, TinyUrlRecordInterface>(_store, Kernel, TinyUrlRecordInterfaceExtensions.CopyFieldsFrom);
            RepoCoreUtil.FindAggregateEntities<GenericQueryServiceInterface, TinyUrlRecord, TinyUrlRecordInterface>(_store, Kernel, TinyUrlRecordInterfaceExtensions.CopyFieldsFrom);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            
        }

        [Test]
        public void TinyUrlJobActionGenerates500UrlsIfCurrentUrlCountIsLessThan5000Test()
        {
            
            var test = new TestQueue();
            var sub = new TinyUrlGenerationJobAction(test);
            var job = new RepeatJob();
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            sub.Run(job);
            Trace.TraceInformation("TinyUrlJobAction generation time " + stopWatch.ElapsedMilliseconds);
            Assert.That(test.ApproximateMessageCount.Value, Is.EqualTo(500));
            QueueMessageInterface msg = null;
            var noDups = new HashSet<string>();
            while (test.Storage.Count > 400)
            {
                test.Storage.TryDequeue(out msg);
                var ret = System.Text.Encoding.ASCII.GetString(msg.Bytes);
                Assert.IsFalse(noDups.Contains(ret));
                noDups.Add(ret);
            }

            sub.Run(job);
            Assert.That(test.Storage.Count, Is.GreaterThanOrEqualTo(900));
            while (test.Storage.Count > 0)
            {
                test.Storage.TryDequeue(out msg);
                var ret = System.Text.Encoding.ASCII.GetString(msg.Bytes);
                Assert.IsFalse(noDups.Contains(ret));
                noDups.Add(ret);
            }
        }

    }
}
