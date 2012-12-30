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
            public void Run(JobBase job)
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
                     JobActionClass = typeof(TestAction),
                     InProgress = true,
                     LastRun = DateTimeOffset.UtcNow
                 };

            var ser = JsonConvert.SerializeObject(job);
            var ret = JsonConvert.DeserializeObject(ser, typeof(RepeatJob)) as RepeatJob;

            Assert.NotNull(ret.JobActionClass);
            Assert.AreEqual(ret.JobActionClass, job.JobActionClass);
            Assert.AreEqual(ret.FriendlyId, job.FriendlyId);
            Assert.AreEqual(ret.InProgress, job.InProgress);
            Assert.AreEqual(ret.LastRun, job.LastRun);
        }
    }
}
