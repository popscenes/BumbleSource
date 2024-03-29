﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Website.Application.Binding;
using Website.Application.Schedule.Command;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;

namespace Website.Application.Schedule
{
    public interface SchedulerInterface
    {
        int RunInterval { get; set; }
        List<JobBase> Jobs { get; }
        void Run(CancellationTokenSource cancellationTokenSource);
        string SchedulerIdentifier { get; set; }
    }

    public class Scheduler : SchedulerInterface
    {
        private readonly GenericQueryServiceInterface _genericQueryService;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericRepositoryInterface _repository;
        private readonly TimeServiceInterface _timeService;
        private readonly MessageBusInterface _messageBus;

        public Scheduler(GenericQueryServiceInterface genericQueryService          
                         , GenericRepositoryInterface repository
                         , UnitOfWorkFactoryInterface unitOfWorkFactory
                         , TimeServiceInterface timeService
                         , [WorkerCommandBus]MessageBusInterface messageBus)
        {
            _genericQueryService = genericQueryService;
            _unitOfWorkFactory = unitOfWorkFactory;
            _repository = repository;
            _timeService = timeService;
            _messageBus = messageBus;
            Jobs = new List<JobBase>();
            RunInterval = 60000;
        }

        public int RunInterval { get; set; }
        public List<JobBase> Jobs { get; private set; }


        public void Run(CancellationTokenSource cancellationTokenSource)
        {
            Init();
            while (!cancellationTokenSource.IsCancellationRequested)
            {           
                CheckRun();
                Thread.Sleep(RunInterval);
            }
            
        }

        public string SchedulerIdentifier { get; set; }

        private void CheckRun()
        {
            var jobList = Jobs.Select(@base => new {@base.Id, type = @base.GetType()}).ToList();
            foreach (var job in jobList.Select(job => _genericQueryService.FindById(job.type, job.Id) as JobBase).Where(j => j != null))
            {
                Replace(job);
                if (!job.IsRunDue(_timeService)) continue;
                
                var commandJobCommand = new JobCommand()
                    {
                        JobId = job.Id,
                        JobType = job.GetType(),
                        SchedulerInfo = "Scheduled By" + SchedulerIdentifier
                    };

                Trace.TraceInformation("Scheduling job {0} by scheduler {1}. Date ", job.Id, SchedulerIdentifier, DateTime.UtcNow);
                _messageBus.Send(commandJobCommand);
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
                    var exist = _genericQueryService.FindById(job.GetType(), job.Id);
                    if (exist == null)
                    {
                        job.CalculateNextRunFromNow(_timeService);
                        _repository.Store(job);
                    }
                    else
                    {
                        Action<object> updateAction = o =>
                            {
                                var update = o as JobBase;
                                update.CurrentProcessor = Guid.Empty;
                                update.InProgress = false;
                            };
                        _repository.UpdateEntity(exist.GetType(), exist.Id, updateAction);
                    }
                        

                    
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
    }
}