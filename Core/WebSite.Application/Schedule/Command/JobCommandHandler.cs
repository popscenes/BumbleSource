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
            try
            {
                var action = _resolutionRoot.Get(command.JobBase.JobActionCommandClass) as JobActionInterface;
                action.Run(command.JobBase);
            }
            catch (Exception e)
            {
                Trace.TraceError("Schedule Job Error {0}, {1}, {2}", command.JobBase.Id, e.Message, e.StackTrace);
            }

            command.JobBase.InProgress = false;
            command.JobBase.LastDuration = _timeService.GetCurrentTime() - command.JobBase.LastRun; 
            var uow = _unitOfWorkFactory.GetUnitOfWork(new[] {_genericRepository});
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