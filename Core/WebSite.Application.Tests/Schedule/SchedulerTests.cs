using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Moq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Schedule;
using Website.Application.Schedule.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using Website.Test.Common;

namespace Website.Application.Tests.Schedule
{
    [TestFixture]
    public class SchedulerTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Bind<CommandBusInterface>()
                  .To<DefaultCommandBus>();

            var store = RepoCoreUtil.GetMockStore<JobInterface>();

            RepoCoreUtil.SetupRepo<GenericRepositoryInterface, JobBase, JobInterface, JobInterface>(store, Kernel, JobInterfaceExtensions.CopyFieldsFrom);

            RepoCoreUtil.SetupRepo<GenericRepositoryInterface, RepeatJob, RepeatJobInterface, JobInterface>(store, Kernel, RepeatJobInterfaceExtensions.CopyFieldsFrom);
            RepoCoreUtil.SetupQueryService<GenericQueryServiceInterface, RepeatJob, RepeatJobInterface, JobInterface>(store, Kernel, RepeatJobInterfaceExtensions.CopyFieldsFrom);

            RepoCoreUtil.SetupRepo<GenericRepositoryInterface, AbsoluteRepeatJob, AbsoluteRepeatJobInterface, JobInterface>(store, Kernel, AbsoluteRepeatJobInterfaceExtensions.CopyFieldsFrom);
            RepoCoreUtil.SetupQueryService<GenericQueryServiceInterface, AbsoluteRepeatJob, AbsoluteRepeatJobInterface, JobInterface>(store, Kernel, AbsoluteRepeatJobInterfaceExtensions.CopyFieldsFrom);
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<CommandBusInterface>();
        }

        [Test]
        public void JobCommandHandlerSetsInProgressToFalseAtTheEndOfJobCompletion()
        {
            var repo = Kernel.Get<GenericRepositoryInterface>();
            var job = new RepeatJob()
            {
                Id = "Every 2 Seconds",
                FriendlyId = "Every 2 Seconds",
                JobActionClass = typeof(TestJobAction),
                RepeatSeconds = 2,
                InProgress = false
            };
            repo.Store(job);
            var qs = Kernel.Get<GenericQueryServiceInterface>();
            

            var jobret = qs.FindById<RepeatJob>(job.Id);
            Assert.IsNotNull(jobret);

            var commmandHandler = Kernel.Get<JobCommandHandler>();
            commmandHandler.Handle(new JobCommand() {JobId = job.Id, JobType = typeof(RepeatJob)});

            jobret = qs.FindById<RepeatJob>(job.Id);
            Assert.IsNotNull(jobret);
            Assert.That(jobret.InProgress, Is.False);
        }

        [Test]
        public void RepeatJobRepeatsAJobForEvery1SecondsTest()
        {
            var watch = new Stopwatch();
            var cancellationTokenSource = new CancellationTokenSource();
            var commandCount = 0;

            Kernel.Bind<TestJobAction>()
                  .ToMethod(context => 
                      new TestJobAction(commandCount++, cancellationTokenSource, null, 2))
                  .InTransientScope();

            var sub = Kernel.Get<Scheduler>();
            sub.RunInterval = 50;
            var job = new RepeatJob()
                {
                    Id = "Every 1 Seconds",
                    FriendlyId = "Every 1 Seconds",
                    JobActionClass = typeof(TestJobAction),
                    RepeatSeconds = 1
                };

            sub.Jobs.Add(job);
            watch.Start();
            sub.Run(cancellationTokenSource);
            Assert.That( watch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(1000));

        }

        [Test]
        public void AbsoluteRepeatJobRepeatsAtDailyIntervalTest()
        {
            var currentTime = new DateTimeOffset(2013, 8, 11, 11, 1, 0, new TimeSpan());

            Kernel.Unbind<TimeServiceInterface>();
            var timeService = Kernel.GetMock<TimeServiceInterface>();
            timeService.Setup(service => service.GetCurrentTime()).Returns(
                () => 
                    currentTime);
            Kernel.Rebind<TimeServiceInterface>().ToConstant(timeService.Object);

            var job = new AbsoluteRepeatJob()
            {
                Id = "Every Day At 11am",
                FriendlyId = "Every Day At 11am",
                JobActionClass = typeof(TestJobAction),
                DayOfWeek = AbsoluteRepeatJob.All,
                HourOfDay = "11"
            };

            var ts = Kernel.Get<TimeServiceInterface>();
            job.CalculateNextRunFromNow(ts);
            Assert.That(job.NextRun, Is.EqualTo(new DateTimeOffset(2013, 8, 12, 11, 0, 0, new TimeSpan())));

            currentTime = currentTime.AddDays(1);
            job.CalculateNextRunFromNow(ts);
            Assert.That(job.NextRun, Is.EqualTo(new DateTimeOffset(2013, 8, 13, 11, 0, 0, new TimeSpan())));

            currentTime = currentTime.AddMinutes(-10);
            job.CalculateNextRunFromNow(ts);
            Assert.That(job.NextRun, Is.EqualTo(new DateTimeOffset(2013, 8, 12, 11, 0, 0, new TimeSpan())));

            Kernel.Rebind<TimeServiceInterface>().To<DefaultTimeService>();
        }

        [Test] public void AbsoluteRepeatJobRepeatsAtHourlyIntervalMondayWednesdayTest()
        {
            var currentTime = new DateTimeOffset(2013, 8, 11, 11, 1, 0, new TimeSpan());

            Kernel.Unbind<TimeServiceInterface>();
            var timeService = Kernel.GetMock<TimeServiceInterface>();
            timeService.Setup(service => service.GetCurrentTime()).Returns(
                () => 
                    currentTime);
            Kernel.Rebind<TimeServiceInterface>().ToConstant(timeService.Object);

            var job = new AbsoluteRepeatJob()
            {
                Id = "Every Hour Monday and Wednesday",
                FriendlyId = "Every Hour Monday and Wednesday",
                JobActionClass = typeof(TestJobAction),
                DayOfWeek = AbsoluteRepeatJob.GetDaysOfWeekStringFor(DayOfWeek.Monday, DayOfWeek.Wednesday),
                HourOfDay = AbsoluteRepeatJob.All,
                Minute = "0"
            };

            var ts = Kernel.Get<TimeServiceInterface>();

            for(var expectedHr = 0; expectedHr < 24; expectedHr++)
            {
                
                job.CalculateNextRunFromNow(ts);
                Assert.That(job.NextRun, Is.EqualTo(new DateTimeOffset(2013, 8, 12, expectedHr, 0, 0, new TimeSpan())));
                currentTime = job.NextRun.AddMinutes(10);
            }

            for (var expectedHr = 0; expectedHr < 24; expectedHr++)
            {
                job.CalculateNextRunFromNow(ts);
                Assert.That(job.NextRun, Is.EqualTo(new DateTimeOffset(2013, 8, 14, expectedHr, 0, 0, new TimeSpan())));
                currentTime = job.NextRun.AddMinutes(10);
            }

            job.CalculateNextRunFromNow(ts);
            Assert.That(job.NextRun, Is.EqualTo(new DateTimeOffset(2013, 8, 19, 0, 0, 0, new TimeSpan())));

            Kernel.Rebind<TimeServiceInterface>().To<DefaultTimeService>();
        }

        public class TestJobAction : JobActionInterface 
        {
            private readonly int _count;
            private readonly CancellationTokenSource _cancel;
            private readonly Action _callback;
            private readonly int _cancelAfter;

            public TestJobAction(int count, CancellationTokenSource cancel, Action callback, int cancelAfter)
            {
                _count = count;
                _cancel = cancel;
                _callback = callback;
                _cancelAfter = cancelAfter;
            }

            public void Run(JobBase job)
            {
                Assert.IsTrue(job.InProgress);
                if (_count >= _cancelAfter && _cancelAfter > 0)
                    _cancel.Cancel();
                if (_callback != null)
                    _callback();
            }
        }
    }
}
