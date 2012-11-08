using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Command
{
    internal class AddFlierToBoardCommandHandler : CommandHandlerInterface<AddFlierToBoardCommand>
    {

        private readonly GenericRepositoryInterface _repository;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly DomainEventPublicationServiceInterface _domainEventPublicationService;


        public AddFlierToBoardCommandHandler(GenericRepositoryInterface repository
            , GenericQueryServiceInterface queryService
            , UnitOfWorkFactoryInterface unitOfWorkFactory, DomainEventPublicationServiceInterface domainEventPublicationService)
        {
            _repository = repository;
            _queryService = queryService;
            _unitOfWorkFactory = unitOfWorkFactory;
            _domainEventPublicationService = domainEventPublicationService;
        }

        public object Handle(AddFlierToBoardCommand command)
        {
            if (command.BoardFliers
                .Select(boardFlier => _queryService
                    .FindById<Flier.Flier>(boardFlier.FlierId))
                    .Any(flier => flier == null || flier.BrowserId != command.BrowserId))
            {
                return new MsgResponse("Invalid Flier for board", true)
                    .AddCommandId(command);
            }

            var boardFliers = new List<BoardFlier>();
            var unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new[] { _repository });
            using (unitOfWork)
            {
                foreach (var boardFlier in command.BoardFliers)
                {            
                    var board = _queryService.FindById<Board>(boardFlier.AggregateId);
                    if (board == null)
                    {
                        continue;
                    }

                    if(board.BrowserId == command.BrowserId)
                    {
                        boardFlier.Status = BoardFlierStatus.Approved;
                    }
                    else if(board.AllowOthersToPostFliers)
                    {
                        boardFlier.Status = board.RequireApprovalOfPostedFliers
                                                ? BoardFlierStatus.PendingApproval
                                                : BoardFlierStatus.Approved;
                    }
                    else
                    {
                        continue;
                    }

                    boardFlier.Id = boardFlier.FlierId + boardFlier.AggregateId;
                    boardFlier.AggregateId = board.Id;
                    boardFlier.DateAdded = DateTime.UtcNow;
                    _repository.Store(boardFlier);
                    boardFliers.Add(boardFlier);
                }    
            }

            if (!unitOfWork.Successful)
                return new MsgResponse("Adding Fliers To Boards Failed", true)
                        .AddCommandId(command);

            foreach (var boardFlier in boardFliers)
            {
                _domainEventPublicationService.Publish(boardFlier);
            }

            return new MsgResponse("Added Fliers To Boards", false)
                .AddCommandId(command);

        }
    }
}