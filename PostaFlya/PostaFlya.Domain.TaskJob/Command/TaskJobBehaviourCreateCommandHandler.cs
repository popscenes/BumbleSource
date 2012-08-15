using System.Collections.Generic;
using PostaFlya.Domain.Flier.Query;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.TaskJob.Command
{
    internal class TaskJobBehaviourCreateCommandHandler : CommandHandlerInterface<TaskJobBehaviourCreateCommand>
    {
        private readonly FlierQueryServiceInterface _flierQueryService;
        private readonly TaskJobRepositoryInterface _taskJobRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public TaskJobBehaviourCreateCommandHandler(FlierQueryServiceInterface flierQueryService, TaskJobRepositoryInterface taskJobRepository, UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _flierQueryService = flierQueryService;
            _taskJobRepository = taskJobRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(TaskJobBehaviourCreateCommand command)
        {
            var flier = _flierQueryService.FindById(command.FlierId);
            if (flier == null)
                return new MsgResponse("Task Job Create Failed", true)
                    .AddCommandId(command)
                    .AddEntityIdError(command.FlierId);

            if(flier.BrowserId != command.BrowserId)
                return new MsgResponse("Task Job Create Failed", true)
                    .AddEntityId(command.FlierId)
                    .AddMessageProperty("Detail", "Flier owner mismatch")
                    .AddCommandId(command);
                    

            var taskJobBehaviour = new TaskJobFlierBehaviour()
                                       {
                                           Id = command.FlierId,
                                           Flier = flier,
                                           MaxAmount = command.MaxAmount,
                                           CostOverhead = command.CostOverhead,
                                           ExtraLocations = command.ExtraLocations
                                       };

            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new List<RepositoryInterface> {_taskJobRepository}))
            {
                _taskJobRepository.Store(taskJobBehaviour);
            }

            if(!unitOfWork.Successful)
                return new MsgResponse("Task Job Create Failed", true)
                    .AddEntityId(command.FlierId)
                    .AddMessageProperty("Detail", "Store Failed")
                    .AddCommandId(command);

            return new MsgResponse("Task Job Create", false)
                .AddEntityId(taskJobBehaviour.Id)
                .AddCommandId(command);
        }
    }
}