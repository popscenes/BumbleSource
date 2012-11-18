using System;
using System.Collections.Generic;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.TaskJob.Command
{
    internal class CreateTaskJobBidCommandHandler : CommandHandlerInterface<CreateTaskJobBidCommand>
    {
        private readonly GenericRepositoryInterface _taskJobRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public CreateTaskJobBidCommandHandler(GenericRepositoryInterface taskJobRepository, UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _taskJobRepository = taskJobRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(CreateTaskJobBidCommand command)
        {
            var uow = _unitOfWorkFactory.GetUnitOfWork(new List<RepositoryInterface>() {_taskJobRepository});
            using (uow)
            {
                _taskJobRepository.UpdateEntity<TaskJobFlierBehaviour>(command.TaskJobId,
                    behaviour => behaviour.Bids.Add(new TaskJobBid()
                        {
                            Id = Guid.NewGuid().ToString(),
                            TaskJobId = command.TaskJobId,
                            BrowserId = command.BrowserId,
                            BidAmount = command.BidAmount
                        }));

            }

            if (!uow.Successful)
                return new MsgResponse("TaskJob Bid Failed", true)
                    .AddEntityId(command.TaskJobId)
                    .AddCommandId(command);

            return new MsgResponse("TaskJob Bid", false)
                .AddEntityId(command.TaskJobId)
                .AddCommandId(command);
        }
    }
}