using System.Diagnostics;
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

            RepoCoreUtil.SetupRepo<GenericRepositoryInterface, RepeatJob, RepeatJobInterface, JobInterface>(store, Kernel, RepeatJobInterfaceExtensions.CopyFieldsFrom);
            RepoCoreUtil.SetupRepo<GenericRepositoryInterface, JobBase, JobInterface, JobInterface>(store, Kernel, JobInterfaceExtensions.CopyFieldsFrom);
            RepoCoreUtil.SetupQueryService<GenericQueryServiceInterface, RepeatJob, RepeatJobInterface, JobInterface>(store, Kernel, RepeatJobInterfaceExtensions.CopyFieldsFrom);
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
                InProgress = true
            };
            repo.Store(job);
            var qs = Kernel.Get<GenericQueryServiceInterface>();
            

            var jobret = qs.FindById<RepeatJob>(job.Id);
            Assert.IsNotNull(jobret);
            Assert.That(jobret.InProgress, Is.True);

            var commmandHandler = Kernel.Get<JobCommandHandler>();
            commmandHandler.Handle(new JobCommand() {JobId = job.Id});

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
                      new TestJobAction(commandCount++, cancellationTokenSource))
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

            Kernel.Unbind<CommandHandlerInterface<JobCommand>>();
        }

        public class TestJobAction : JobActionInterface 
        {
            private readonly int _count;
            private readonly CancellationTokenSource _cancel;

            public TestJobAction(int count, CancellationTokenSource cancel)
            {
                _count = count;
                _cancel = cancel;
            }

            public void Run(JobBase job)
            {
                if(_count > 0)
                    _cancel.Cancel();
            }
        }
    }
}
