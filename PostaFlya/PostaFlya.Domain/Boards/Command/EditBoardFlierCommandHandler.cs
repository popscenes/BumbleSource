using PostaFlya.Domain.Boards.Event;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Command
{
    internal class EditBoardFlierCommandHandler : CommandHandlerInterface<EditBoardFlierCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly DomainEventPublishServiceInterface _domainEventPublishService;


        public EditBoardFlierCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory, GenericQueryServiceInterface queryService, GenericRepositoryInterface repository, DomainEventPublishServiceInterface domainEventPublishService)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _queryService = queryService;
            _repository = repository;
            _domainEventPublishService = domainEventPublishService;
        }

        public object Handle(EditBoardFlierCommand command)
        {
            var boardFlier = _queryService.FindByAggregate<BoardFlier>(command.FlierId + command.BoardId, command.BoardId);
            if(boardFlier == null)
                return new MsgResponse("Updating Flier On Board Failed, Flier not on board", true)
                    .AddCommandId(command);
            var board = _queryService.FindById<Board>(command.BoardId);
            if(board.BrowserId != command.BrowserId)
                return new MsgResponse("Updating Flier On Board Failed, browser doesn't own board", true)
                    .AddCommandId(command);

            var unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new[] { _repository });
            using (unitOfWork)
            {
                _repository.UpdateAggregateEntity<BoardFlier>(command.FlierId + command.BoardId, command.BoardId, bf =>
                    {
                        bf.Status = command.Status;
                    });
            }
            if (!unitOfWork.Successful)
                return new MsgResponse("Updating Flier On Board Status Failed", true)
                        .AddCommandId(command);

            _domainEventPublishService.Publish(new BoardFlierModifiedEvent()
                {
                    OrigState = boardFlier,
                    NewState = _queryService.FindByAggregate<BoardFlier>(command.FlierId + command.BoardId, command.BoardId)
                }
            );

            return new MsgResponse("Updated Flier On Board Status", false)
                .AddCommandId(command);
        }
    }
}