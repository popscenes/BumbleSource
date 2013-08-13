using System;
using System.Collections.Generic;
using PostaFlya.Domain.Boards.Event;
using PostaFlya.Domain.Boards.Query;
using PostaFlya.Domain.Venue;
using Website.Domain.Service;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Command
{
    public class CreateBoardCommandHandler : MessageHandlerInterface<CreateBoardCommand>
    {
        private readonly GenericRepositoryInterface _boardRepository;
        private readonly GenericQueryServiceInterface _boardQueryService;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly EventPublishServiceInterface _eventPublishService;
        private readonly MessageBusInterface _messageBus;
        private readonly QueryChannelInterface _queryChannel;

        public CreateBoardCommandHandler(GenericRepositoryInterface boardRepository
            , GenericQueryServiceInterface boardQueryService
            , UnitOfWorkFactoryInterface unitOfWorkFactory
            , EventPublishServiceInterface eventPublishService
            , [WorkerCommandBus]MessageBusInterface messageBus, QueryChannelInterface queryChannel)
        {
            _boardRepository = boardRepository;
            _boardQueryService = boardQueryService;
            _unitOfWorkFactory = unitOfWorkFactory;
            _eventPublishService = eventPublishService;
            _messageBus = messageBus;
            _queryChannel = queryChannel;
        }

        public object Handle(CreateBoardCommand command)
        {

            var newBoard = GetNewBoard(command);
            var unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new object[] { _boardRepository, _boardQueryService });
            using (unitOfWork)
            {
                newBoard.FriendlyId = _queryChannel.FindFreeFriendlyId(newBoard);
                _boardRepository.Store(newBoard);
            }

            if (!unitOfWork.Successful)
                return new MsgResponse("Board Creation Failed", true)
                        .AddCommandId(command);

            _eventPublishService.Publish(new BoardModifiedEvent(){NewState = newBoard});

            /*if (!string.IsNullOrWhiteSpace(command.FlierIdToAddOnCreate))
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
            }*/

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
                    BoardTypeEnum = command.BoardTypeEnum,
                    AdminEmailAddresses = command.AdminEmailAddresses,
                    Description = command.Description

                };

            if (newBoard.BoardTypeEnum != BoardTypeEnum.InterestBoard)
            {

                if (command.SourceInformation != null)
                {
                    newBoard.InformationSources = new List<VenueInformation>() { command.SourceInformation };
                    newBoard.DefaultInformationSource = command.SourceInformation.Source;
                }
                    
            }
            return newBoard;
        }
    }
}