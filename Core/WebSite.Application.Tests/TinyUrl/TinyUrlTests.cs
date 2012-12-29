using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Ninject.MockingKernel.Moq;
using Website.Application.Binding;
using Website.Application.Queue;
using Website.Application.Schedule;
using Website.Application.Tests.Mocks;

namespace Website.Application.Tests.TinyUrl
{
    [TestFixture]
    public class TinyUrlTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {

        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            
        }

        [Test]
        public void TinyUrlJobActionGenerates10000UrlsIfCurrentUrlCountIsLessThan5000Test()
        {
            var stopWatch = new Stopwatch();
            var test = new TestQueue();
            var sub = new TinyUrlJobAction(test);
            var job = new RepeatJob();
            stopWatch.Start();
            sub.Run(job);
            Trace.TraceInformation("" + stopWatch.ElapsedMilliseconds);
            Assert.That(test.ApproximateMessageCount.Value, Is.EqualTo(10000));
            QueueMessageInterface msg = null;
            while (test.Storage.Count > 9700)
            {
                test.Storage.TryDequeue(out msg);
                var ret = System.Text.Encoding.ASCII.GetString(msg.Bytes);
                Trace.TraceInformation(ret);
            }

        }

    }
}
