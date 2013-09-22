using System;
using System.Diagnostics;
using Ninject;
using Ninject.Syntax;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;

namespace Website.Application.Schedule.Command
{
    public class JobCommandHandler : MessageHandlerInterface<JobCommand>
    {
        private readonly UnitOfWorkInterface _unitOfWorkFactory;
        private readonly GenericRepositoryInterface _genericRepository;
        private readonly GenericQueryServiceInterface _genericQueryService;
        private readonly IResolutionRoot _resolutionRoot;
        private readonly TimeServiceInterface _timeService;
        readonly Stopwatch _stopWatch = new Stopwatch();

        public JobCommandHandler(UnitOfWorkInterface unitOfWorkFactory
                                 , GenericRepositoryInterface genericRepository, IResolutionRoot resolutionRoot, TimeServiceInterface timeService, GenericQueryServiceInterface genericQueryService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _genericRepository = genericRepository;
            _resolutionRoot = resolutionRoot;
            _timeService = timeService;
            _genericQueryService = genericQueryService;
        }

        public void Handle(JobCommand command)
        {
            var job = _genericQueryService.FindById(command.JobType, command.JobId) as JobBase;
            if (job.InProgress && !job.IsTimedOut(_timeService))
                return;
            if (!job.IsRunDue(_timeService))
                return;

            job.CurrentProcessor = Guid.NewGuid();
            job.LastRun = _timeService.GetCurrentTime();
            job.InProgress = true;
            job.CalculateNextRunFromNow(_timeService);
            JobBase currentState = null;
            using (_unitOfWorkFactory.Begin())
            {
                _genericRepository.UpdateEntity(command.JobType, command.JobId, o =>
                {
                    currentState = o as JobBase;
                    if (currentState.CurrentProcessor == Guid.Empty)
                        currentState.CopyState(job);
                });
            }

            if (currentState.CurrentProcessor != job.CurrentProcessor)
            {
                Trace.TraceWarning("another job command already in progress {0}", command.JobId);
                return;
            }

            _stopWatch.Start();
            try
            {

                var action = _resolutionRoot.Get(job.JobActionClass) as JobActionInterface;

                Trace.TraceInformation("Schedule Job {0} Start at {1}. type: {2}, ScheduledInfo: {3} "
                    , command.JobId, action.GetType().ToString(), _stopWatch.ElapsedMilliseconds, command.SchedulerInfo);

                action.Run(job);

                Trace.TraceInformation("Schedule Job {0} Completed. type: {1}, time(ms): {2}, ScheduledInfo: {3}"
                    , command.JobId, action.GetType().ToString(), _stopWatch.ElapsedMilliseconds, command.SchedulerInfo);
            }
            catch (Exception e)
            {
                Trace.TraceError("Schedule Job {0} Error, time(ms): {3}\n msg:{1} \n {2}", command.JobId, e.Message, e.StackTrace, _stopWatch.ElapsedMilliseconds);
            }

            job.InProgress = false;
            job.LastDuration = TimeSpan.FromMilliseconds(_stopWatch.ElapsedMilliseconds); 
            using (_unitOfWorkFactory.Begin())
            {
                _genericRepository.UpdateEntity(command.JobType, command.JobId, o =>
                    {
                        var update = o as JobBase;
                        update.CopyState(job);
                        update.CurrentProcessor = Guid.Empty;
                    });
            }
        }
    }
}