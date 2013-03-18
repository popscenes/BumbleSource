using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Ninject;
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

//        [Test]
//        public void TinyUrlJobActionGenerates500UrlsIfCurrentUrlCountIsLessThan5000Test()
//        {
//
//            var sub = Kernel.Get<TinyUrlGenerationJobAction>();
//            var job = new RepeatJob();
//            var stopWatch = new Stopwatch();
//            stopWatch.Start();
//            sub.Run(job);
//            sub.Run(job);
//            sub.Run(job);
//            sub.Run(job);
//            sub.Run(job);
//            Trace.TraceInformation("TinyUrlJobAction generation time " + stopWatch.ElapsedMilliseconds);
//            Assert.That(_store.Count, Is.EqualTo(DefaultTinyUrlService.TinyUrlsToBuffer));
//            var noDups = new HashSet<string>();
//            while (_store.Count > (DefaultTinyUrlService.TinyUrlsToBuffer - (DefaultTinyUrlService.TinyUrlsToBuffer/2)))
//            {
//                var @enum = _store.GetEnumerator();
//                @enum.MoveNext();
//                var rem = @enum.Current;
//                noDups.Add(rem.TinyUrl);
//                _store.Remove(rem);
//            }
//
//            sub.Run(job);
//            sub.Run(job);
//            sub.Run(job);
//            sub.Run(job);
//            sub.Run(job);
//            Assert.That(_store.Count, Is.EqualTo(DefaultTinyUrlService.TinyUrlsToBuffer));
//            while (_store.Count > 0)
//            {
//                var @enum = _store.GetEnumerator();
//                @enum.MoveNext();
//                var rem = @enum.Current;
//                Assert.IsFalse(noDups.Contains(rem.TinyUrl));
//                noDups.Add(rem.TinyUrl);
//                _store.Remove(rem);
//
//            }
//        }

    }
}
