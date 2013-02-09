using System;
using System.Diagnostics;
using Ninject;
using Ninject.Syntax;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace Website.Application.Schedule.Command
{
    public class JobCommandHandler : CommandHandlerInterface<JobCommand>
    {
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericRepositoryInterface _genericRepository;
        private readonly GenericQueryServiceInterface _genericQueryService;
        private readonly IResolutionRoot _resolutionRoot;
        private readonly TimeServiceInterface _timeService;
        readonly Stopwatch _stopWatch = new Stopwatch();

        public JobCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory
                                 , GenericRepositoryInterface genericRepository, IResolutionRoot resolutionRoot, TimeServiceInterface timeService, GenericQueryServiceInterface genericQueryService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _genericRepository = genericRepository;
            _resolutionRoot = resolutionRoot;
            _timeService = timeService;
            _genericQueryService = genericQueryService;
        }

        public object Handle(JobCommand command)
        {
            var job = _genericQueryService.FindById(command.JobType, command.JobId) as JobBase;
            if (job.InProgress)
                return true;
            job.LastRun = _timeService.GetCurrentTime();
            job.InProgress = true;
            job.CalculateNextRunFromNow(_timeService);
            var uow = _unitOfWorkFactory.GetUnitOfWork(new[] { _genericRepository });
            using (uow)
            {
                _genericRepository.UpdateEntity(command.JobType, command.JobId, o =>
                {
                    var update = o as JobBase;
                    update.CopyState(job);
                });
            }

            if (!uow.Successful)
            {
                Trace.TraceError("Failed to update Schedule Job {0} state", command.JobId);
                return false;
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
            uow = _unitOfWorkFactory.GetUnitOfWork(new[] {_genericRepository});
            using (uow)
            {
                _genericRepository.UpdateEntity(command.JobType, command.JobId, o =>
                    {
                        var update = o as JobBase;
                        update.CopyState(job);
                    });
            }

            return uow.Successful;
        }
    }
}