using System;
using System.Collections.Generic;
using PostaFlya.Domain.Boards.Event;
using PostaFlya.Domain.Boards.Query;
using PostaFlya.Domain.Venue;
using Website.Domain.Service;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Command
{
    public class CreateBoardCommandHandler : CommandHandlerInterface<CreateBoardCommand>
    {
        private readonly GenericRepositoryInterface _boardRepository;
        private readonly GenericQueryServiceInterface _boardQueryService;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly DomainEventPublishServiceInterface _domainEventPublishService;
        private readonly CommandBusInterface _commandBus;

        public CreateBoardCommandHandler(GenericRepositoryInterface boardRepository
            , GenericQueryServiceInterface boardQueryService
            , UnitOfWorkFactoryInterface unitOfWorkFactory
            , DomainEventPublishServiceInterface domainEventPublishService
            , [WorkerCommandBus]CommandBusInterface commandBus)
        {
            _boardRepository = boardRepository;
            _boardQueryService = boardQueryService;
            _unitOfWorkFactory = unitOfWorkFactory;
            _domainEventPublishService = domainEventPublishService;
            _commandBus = commandBus;
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

            if (!string.IsNullOrWhiteSpace(command.FlierIdToAddOnCreate))
            {
                _commandBus.Send(new AddFlierToBoardCommand()
                {
                    
                    BoardFliers = new List<BoardFlier>() {new BoardFlier()
                        {
                            AggregateId = newBoard.Id,
                            FlierId = command.FlierIdToAddOnCreate,
                            Status = BoardFlierStatus.PendingApproval
                            
                        }}
                });
            }

            return new MsgResponse("Board Create", false)
                .AddEntityId(newBoard.Id)
                .AddCommandId(command);
        }

        private static Board GetNewBoard(CreateBoardCommand command)
        {
            var newBoard = new Board
                {
                    BrowserId = command.BrowserId,
                    Id = Guid.NewGuid().ToString(),
                    FriendlyId = command.BoardName,
                    Name = command.BoardName,
                    RequireApprovalOfPostedFliers = command.RequireApprovalOfPostedFliers,
                    AllowOthersToPostFliers = command.AllowOthersToPostFliers,
                    Status = BoardStatus.Approved,
                    BoardTypeEnum = command.BoardTypeEnum
                };

            if (newBoard.BoardTypeEnum != BoardTypeEnum.InterestBoard)
            {
                if (command.Location != null && command.Location.IsValid)
                    newBoard.Location = command.Location;
                
                if (command.SourceInformation != null)
                {
                    newBoard.InformationSources = new List<VenueInformation>() { command.SourceInformation };
                    if (command.SourceInformation.Address.IsValid)
                        newBoard.Location = command.SourceInformation.Address;
                }
                    
            }
            return newBoard;
        }
    }
}