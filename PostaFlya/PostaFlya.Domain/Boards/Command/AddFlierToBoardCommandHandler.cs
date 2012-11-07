using System;
using System.Dynamic;
using System.Linq;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Command
{
    internal class AddFlierToBoardCommandHandler : CommandHandlerInterface<AddFlierToBoardCommand>
    {

        private readonly GenericRepositoryInterface _repository;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public AddFlierToBoardCommandHandler(GenericRepositoryInterface repository, GenericQueryServiceInterface queryService, UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _repository = repository;
            _queryService = queryService;
            _unitOfWorkFactory = unitOfWorkFactory;
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

            var unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new[] { _repository });
            using (unitOfWork)
            {
                foreach (var flierBoard in command.BoardFliers)
                {            
                    var board = _queryService.FindById<Board>(flierBoard.AggregateId);
                    if (board == null)
                    {
                        continue;
                    }

                    if(board.BrowserId == command.BrowserId)
                    {
                        flierBoard.Status = BoardFlierStatus.Approved;
                    }
                    else if(board.AllowOthersToPostFliers)
                    {
                        flierBoard.Status = board.RequireApprovalOfPostedFliers
                                                ? BoardFlierStatus.PendingApproval
                                                : BoardFlierStatus.Approved;
                    }
                    else
                    {
                        continue;
                    }

                    flierBoard.Id = flierBoard.FlierId + flierBoard.AggregateId;
                    flierBoard.AggregateId = board.Id;
                    flierBoard.DateAdded = DateTime.UtcNow;
                    _repository.Store(flierBoard);
                }    
            }

            if (!unitOfWork.Successful)
                return new MsgResponse("Adding Fliers To Boards Failed", true)
                        .AddCommandId(command);

            return new MsgResponse("Added Fliers To Boards", false)
                .AddCommandId(command);

        }
    }
}