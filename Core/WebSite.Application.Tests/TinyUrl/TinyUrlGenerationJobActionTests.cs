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
using Website.Application.TinyUrl;

namespace Website.Application.Tests.TinyUrl
{
    [TestFixture]
    public class TinyUrlGenerationJobActionTests
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
