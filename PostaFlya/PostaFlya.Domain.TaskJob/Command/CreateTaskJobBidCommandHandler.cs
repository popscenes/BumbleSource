using System;
using System.Collections.Generic;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace PostaFlya.Domain.TaskJob.Command
{
    internal class CreateTaskJobBidCommandHandler : CommandHandlerInterface<CreateTaskJobBidCommand>
    {
        private readonly TaskJobRepositoryInterface _taskJobRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public CreateTaskJobBidCommandHandler(TaskJobRepositoryInterface taskJobRepository, UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _taskJobRepository = taskJobRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(CreateTaskJobBidCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(new List<RepositoryInterface>() { _taskJobRepository }))
            {
                var current = _taskJobRepository.GetBidForUpdate(command.TaskJobId, command.BrowserId) ??
                                new TaskJobBid()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    TaskJobId = command.TaskJobId,
                                    BrowserId = command.BrowserId,
                                    BidAmount = command.BidAmount
                                };

                current.BidAmount = command.BidAmount;

                if(!_taskJobRepository.BidOnTask(current))
                    return new MsgResponse("TaskJob Bid Failed", true)
                        .AddEntityId(current.TaskJobId)
                        .AddCommandId(command);

                return new MsgResponse("TaskJob Bid", false)
                    .AddEntityId(current.TaskJobId)
                    .AddCommandId(command);
            }
        }
    }
}