using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using PostaFlya.Domain.Boards.Event;
using PostaFlya.Domain.Flier;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Command
{
    public class AddFlierToBoardCommandHandler : CommandHandlerInterface<AddFlierToBoardCommand>
    {

        private readonly GenericRepositoryInterface _repository;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly DomainEventPublishServiceInterface _domainEventPublishService;


        public AddFlierToBoardCommandHandler(GenericRepositoryInterface repository
            , GenericQueryServiceInterface queryService
            , UnitOfWorkFactoryInterface unitOfWorkFactory, DomainEventPublishServiceInterface domainEventPublishService)
        {
            _repository = repository;
            _queryService = queryService;
            _unitOfWorkFactory = unitOfWorkFactory;
            _domainEventPublishService = domainEventPublishService;
        }

        public object Handle(AddFlierToBoardCommand command)
        {
            UnitOfWorkInterface unitOfWork = null;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new[] { _repository }))
            {
                foreach (var boardFlier in command.BoardFliers)
                {
                    var boardId = boardFlier.AggregateId;
                    _repository.UpdateEntity<Domain.Flier.Flier>(boardFlier.FlierId,
                    flier =>
                    {
                        if (flier.Boards == null)
                            flier.Boards = new HashSet<string>();

                        flier.Boards.Add(boardId);
                        
                    });
                }


            }
            if (!unitOfWork.Successful)
                return new MsgResponse("Added Fliers To Boards failed", true)
                    .AddCommandId(command);

            return new MsgResponse("Added Fliers To Boards", false)
                .AddCommandId(command);

        }

    }
}