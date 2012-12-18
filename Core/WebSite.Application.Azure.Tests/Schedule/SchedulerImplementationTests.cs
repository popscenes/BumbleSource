using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Newtonsoft.Json;
using Website.Application.Schedule;

namespace Website.Application.Azure.Tests.Schedule
{
    [TestFixture]
    public class SchedulerImplementationTests
    {
        public class TestAction : JobActionInterface
        {
            public void Run()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void JobClassIsJsonSerializable()
        {
             var  job = new RepeatJob()
                 {
                     FriendlyId = "123",
                     JobActionCommandClass = typeof(TestAction),
                     InProgress = true,
                     LastRun = DateTimeOffset.UtcNow
                 };

            var ser = JsonConvert.SerializeObject(job);
            var ret = JsonConvert.DeserializeObject(ser, typeof(RepeatJob)) as RepeatJob;

            Assert.NotNull(ret.JobActionCommandClass);
            Assert.AreEqual(ret.JobActionCommandClass, job.JobActionCommandClass);
            Assert.AreEqual(ret.FriendlyId, job.FriendlyId);
            Assert.AreEqual(ret.InProgress, job.InProgress);
            Assert.AreEqual(ret.LastRun, job.LastRun);
        }
    }
}
