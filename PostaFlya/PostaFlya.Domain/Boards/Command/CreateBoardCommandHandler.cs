using System;
using PostaFlya.Domain.Boards.Event;
using PostaFlya.Domain.Boards.Query;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Command
{
    internal class CreateBoardCommandHandler : CommandHandlerInterface<CreateBoardCommand>
    {
        private readonly GenericRepositoryInterface _boardRepository;
        private readonly GenericQueryServiceInterface _boardQueryService;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly DomainEventPublishServiceInterface _domainEventPublishService;

        public CreateBoardCommandHandler(GenericRepositoryInterface boardRepository, GenericQueryServiceInterface boardQueryService, UnitOfWorkFactoryInterface unitOfWorkFactory, DomainEventPublishServiceInterface domainEventPublishService)
        {
            _boardRepository = boardRepository;
            _boardQueryService = boardQueryService;
            _unitOfWorkFactory = unitOfWorkFactory;
            _domainEventPublishService = domainEventPublishService;
        }

        public object Handle(CreateBoardCommand command)
        {

            var newBoard = GetNewBoard(command);
            var unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new object[] { _boardRepository, _boardQueryService });
            using (unitOfWork)
            {
                newBoard.FriendlyId = _boardQueryService.FindFreeFriendlyId(newBoard);
                _boardRepository.Store(newBoard);
            }

            if (!unitOfWork.Successful)
                return new MsgResponse("Board Creation Failed", true)
                        .AddCommandId(command);

            _domainEventPublishService.Publish(new BoardModifiedEvent(){NewState = newBoard});

            return new MsgResponse("Board Create", false)
                .AddEntityId(newBoard.Id)
                .AddCommandId(command);
        }

        private static Board GetNewBoard(CreateBoardCommand command)
        {
            var newBoard = command.BoardTypeEnum == BoardTypeEnum.VenueBoard ? new VenueBoard() : new Board();
            newBoard.BrowserId = command.BrowserId;
            newBoard.Id = Guid.NewGuid().ToString();
            newBoard.FriendlyId = command.BoardName;
            newBoard.RequireApprovalOfPostedFliers = command.RequireApprovalOfPostedFliers;
            newBoard.AllowOthersToPostFliers = command.AllowOthersToPostFliers;
            newBoard.Status = BoardStatus.Approved;
            newBoard.BoardTypeEnum = command.BoardTypeEnum;
            return newBoard;
        }
    }
}