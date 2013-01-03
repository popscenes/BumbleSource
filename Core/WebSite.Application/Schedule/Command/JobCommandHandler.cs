using System;
using System.Diagnostics;
using Ninject;
using Ninject.Syntax;
using Website.Infrastructure.Command;

namespace Website.Application.Schedule.Command
{
    public class JobCommandHandler : CommandHandlerInterface<JobCommand>
    {
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericRepositoryInterface _genericRepository;
        private readonly IResolutionRoot _resolutionRoot;
        private readonly TimeServiceInterface _timeService;
        readonly Stopwatch _stopWatch = new Stopwatch();

        public JobCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory
                                 , GenericRepositoryInterface genericRepository, IResolutionRoot resolutionRoot, TimeServiceInterface timeService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _genericRepository = genericRepository;
            _resolutionRoot = resolutionRoot;
            _timeService = timeService;
        }

        public object Handle(JobCommand command)
        {
            if (command.JobBase.InProgress)
                return true;
            command.JobBase.LastRun = _timeService.GetCurrentTime();
            command.JobBase.InProgress = true;
            command.JobBase.CalculateNextRun(_timeService);
            var uow = _unitOfWorkFactory.GetUnitOfWork(new[] { _genericRepository });
            using (uow)
            {
                _genericRepository.UpdateEntity(command.JobBase.GetType(), command.JobBase.Id, o =>
                {
                    var update = o as JobBase;
                    update.CopyState(command.JobBase);
                });
            }

            if (!uow.Successful)
            {
                Trace.TraceError("Failed to update Schedule Job {0} state", command.JobBase.Id);
                return false;
            }

            _stopWatch.Start();
            try
            {                               
                var action = _resolutionRoot.Get(command.JobBase.JobActionClass) as JobActionInterface;
                action.Run(command.JobBase);

                Trace.TraceInformation("Schedule Job {0} Completed. type: {1}, time(ms): {2}", command.JobBase.Id, action.GetType().ToString(), _stopWatch.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                Trace.TraceError("Schedule Job {0} Error, time(ms): {3}\n msg:{1} \n {2}", command.JobBase.Id, e.Message, e.StackTrace, _stopWatch.ElapsedMilliseconds);
            }

            command.JobBase.InProgress = false;
            command.JobBase.LastDuration = TimeSpan.FromMilliseconds(_stopWatch.ElapsedMilliseconds); 
            uow = _unitOfWorkFactory.GetUnitOfWork(new[] {_genericRepository});
            using (uow)
            {
                _genericRepository.UpdateEntity(command.JobBase.GetType(), command.JobBase.Id, o =>
                    {
                        var update = o as JobBase;
                        update.CopyState(command.JobBase);
                    });
            }

            return uow.Successful;
        }
    }
}