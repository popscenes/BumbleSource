using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Command
{
    internal class EditBoardFlierCommandHandler : CommandHandlerInterface<EditBoardFlierCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public EditBoardFlierCommandHandler(UnitOfWorkFactoryInterface unitOfWorkFactory, GenericQueryServiceInterface queryService, GenericRepositoryInterface repository)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _queryService = queryService;
            _repository = repository;
        }

        public object Handle(EditBoardFlierCommand command)
        {
            var boardFlier = _queryService.FindById<BoardFlier>(command.FlierId + command.BoardId);
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
                _repository.UpdateEntity<BoardFlier>(command.FlierId + command.BoardId, bf =>
                    {
                        bf.Status = command.Status;
                    });
            }
            if (!unitOfWork.Successful)
                return new MsgResponse("Updating Flier On Board Status Failed", true)
                        .AddCommandId(command);

            return new MsgResponse("Updated Flier On Board Status", false)
                .AddCommandId(command);
        }
    }
}