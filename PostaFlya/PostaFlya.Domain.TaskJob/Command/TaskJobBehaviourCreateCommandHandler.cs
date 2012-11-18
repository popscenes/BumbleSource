using System.Collections.Generic;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.TaskJob.Command
{
    internal class TaskJobBehaviourCreateCommandHandler : CommandHandlerInterface<TaskJobBehaviourCreateCommand>
    {
        private readonly GenericQueryServiceInterface _queryService;
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public TaskJobBehaviourCreateCommandHandler(GenericQueryServiceInterface queryService, GenericRepositoryInterface repository, UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _queryService = queryService;
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(TaskJobBehaviourCreateCommand command)
        {
            var flier = _queryService.FindById<Flier.Flier>(command.FlierId);
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
                                           FriendlyId = flier.FriendlyId,
                                           Flier = flier,
                                           MaxAmount = command.MaxAmount,
                                           CostOverhead = command.CostOverhead,
                                           ExtraLocations = command.ExtraLocations
                                       };

            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new List<RepositoryInterface> {_repository}))
            {
                _repository.Store(taskJobBehaviour);
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