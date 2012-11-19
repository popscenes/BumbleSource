using PostaFlya.Domain.Boards.Event;
using PostaFlya.Domain.Boards.Query;
using Website.Domain.Browser;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Command
{
    public class EditBoardCommandHandler : CommandHandlerInterface<EditBoardCommand>
    {
        private readonly GenericRepositoryInterface _boardRepository;
        private readonly GenericQueryServiceInterface _boardQueryService;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly DomainEventPublishServiceInterface _domainEventPublishService;

        public EditBoardCommandHandler(GenericRepositoryInterface boardRepository
                                       , GenericQueryServiceInterface boardQueryService
                                       , UnitOfWorkFactoryInterface unitOfWorkFactory
                                       , DomainEventPublishServiceInterface domainEventPublishService)
        {
            _boardRepository = boardRepository;
            _boardQueryService = boardQueryService;
            _unitOfWorkFactory = unitOfWorkFactory;
            _domainEventPublishService = domainEventPublishService;
        }

        public object Handle(EditBoardCommand command)
        {
            var boardExist = _boardQueryService.FindById<Board>(command.Id);
            var brows = _boardQueryService.FindById<Browser>(command.BrowserId);

            if(boardExist == null || (boardExist.BrowserId != command.BrowserId && !brows.HasRole(Role.Admin)))
                return new MsgResponse("Board Edit not allowed", true)
                    .AddCommandId(command);

            if (!brows.HasRole(Role.Admin))
                command.Status = boardExist.Status;

            var unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new object[] { _boardRepository, _boardQueryService });
            using (unitOfWork)
            {
                _boardRepository.UpdateEntity<Board>(command.Id, board =>
                    {
                        board.Status = command.Status;

                        if (!string.IsNullOrWhiteSpace(command.BoardName))
                        {
                            board.FriendlyId = command.BoardName;
                            _boardQueryService.FindFreeFriendlyId(board);
                        }

                        if (!string.IsNullOrWhiteSpace(command.Description))
                        {
                            board.Description = command.Description;
                        }

                        if (command.Location != null && command.Location.IsValid && !command.Location.Equals(board.Location))
                        {
                            if (!brows.HasRole(Role.Admin))
                                board.Status =   BoardStatus.PendingApproval;
                            board.Location = command.Location;
                        }
                        
                    } );
            }

            if (!unitOfWork.Successful)
                return new MsgResponse("Board Edit Failed", true)
                    .AddCommandId(command);

            var newBoard = _boardQueryService.FindById<Board>(command.Id);
            _domainEventPublishService.Publish(new BoardModifiedEvent() { NewState = newBoard, OrigState = boardExist});

            return new MsgResponse("Board Edit Succeded", false)
                .AddEntityId(newBoard.Id)
                .AddCommandId(command);
        }
    }
}