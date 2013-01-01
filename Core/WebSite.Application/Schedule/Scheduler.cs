using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Website.Application.Binding;
using Website.Application.Schedule.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace Website.Application.Schedule
{
    public interface SchedulerInterface
    {
        int RunInterval { get; set; }
        List<JobBase> Jobs { get; }
        void Run(CancellationTokenSource cancellationTokenSource);
    }

    public class Scheduler : SchedulerInterface
    {
        private readonly GenericQueryServiceInterface _genericQueryService;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericRepositoryInterface _repository;
        private readonly TimeServiceInterface _timeService;
        private readonly CommandBusInterface _commandBus;

        public Scheduler(GenericQueryServiceInterface genericQueryService          
                         , GenericRepositoryInterface repository
                         , UnitOfWorkFactoryInterface unitOfWorkFactory
                         , TimeServiceInterface timeService
                         , [WorkerCommandBus]CommandBusInterface commandBus)
        {
            _genericQueryService = genericQueryService;
            _unitOfWorkFactory = unitOfWorkFactory;
            _repository = repository;
            _timeService = timeService;
            _commandBus = commandBus;
            Jobs = new List<JobBase>();
            RunInterval = 30000;
        }

        public int RunInterval { get; set; }
        public List<JobBase> Jobs { get; private set; }


        public void Run(CancellationTokenSource cancellationTokenSource)
        {          
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                Init();
                CheckRun();
                Thread.Sleep(RunInterval);
            }
            
        }

        private void CheckRun()
        {
            foreach (var jobBase in Jobs.Where(jobBase => !jobBase.InProgress && jobBase.IsRunDue(_timeService)))
            {
                jobBase.LastRun = _timeService.GetCurrentTime();
                jobBase.InProgress = true;
                jobBase.CalculateNextRun(_timeService);
                Update();
                var commandJobCommand = new JobCommand()
                {
                    JobBase = jobBase
                };
                _commandBus.Send(commandJobCommand);
            }
        }

        private void Init()
        {
            var uow = _unitOfWorkFactory.GetUnitOfWork(new[] {_repository});
            using (uow)
            {
                for (var i = 0; i < Jobs.Count; i++)
                {
                    dynamic job = Jobs.ElementAt(i);
                    var exist = _genericQueryService.FindById(job.GetType(), job.Id) as JobBase;
                    if (exist != null)
                        Replace(exist);
                    else
                        _repository.Store(job);
                }
            }

            if(!uow.Successful)
                throw new Exception("Failed to initialize job storage");
        }

        private void Replace(JobBase jobBase)
        {
            var job = Jobs.FirstOrDefault(@base => @base.Id == jobBase.Id);
            if (job != null)
                Jobs.Remove(job);
          
            Jobs.Add(jobBase);           
        }

        private void Update()
        {
            using (_unitOfWorkFactory.GetUnitOfWork(new[]{_repository}))
            {
                for (var i = 0; i < Jobs.Count; i++)
                {
                    var job = Jobs.ElementAt(i);
                    _repository.UpdateEntity(job.GetType(), job.Id, o =>
                        {
                            var update = o as JobBase;
                            update.CopyState(job);
                        });

                }    
            }
            
        }
    }
}