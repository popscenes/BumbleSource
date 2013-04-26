using System;
using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Boards.Query;
using PostaFlya.Domain.Venue;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Command
{
    [Serializable]
    public class MatchFlierToBoardsCommand : DefaultCommandBase
    {
        public string FlierId { get; set; }
    }

    internal class MatchFlierToBoardsCommandHandler: CommandHandlerInterface<MatchFlierToBoardsCommand>
    {
        private readonly GenericQueryServiceInterface _queryService;
        private readonly QueryChannelInterface _queryChannel;
        private readonly CommandBusInterface _commandBus;
        public MatchFlierToBoardsCommandHandler(GenericQueryServiceInterface queryService
            , QueryChannelInterface queryChannel, CommandBusInterface commandBus)
        {
            _queryService = queryService;
            _queryChannel = queryChannel;
            _commandBus = commandBus;
        }

        public object Handle(MatchFlierToBoardsCommand command)
        {
            var flier = _queryService.FindById<Flier.Flier>(command.FlierId);
            if (flier == null || string.IsNullOrWhiteSpace(flier.ContactDetails.PlaceName))
                return null;

            var board = _queryChannel.Query(new FindBoardForVenueQuery() { VenueInformation = flier.ContactDetails }, (Board)null);
            if (board == null)
            {
                CreateBoard(flier.ContactDetails, flier.Id);
                return true;
            }
            else if (flier.Boards.Any(s => s == board.Id))
            {
                return true;
            }

            return _commandBus.Send(new AddFlierToBoardCommand()
                {
                    BoardFliers = new List<BoardFlier>() {new BoardFlier()
                        {
                            AggregateId = board.Id,
                            FlierId = flier.Id,
                            Status = BoardFlierStatus.PendingApproval 
                        }}
                });

        }

        private void CreateBoard(VenueInformation source, string flierId)
        {
            var createBoardCommand = new CreateBoardCommand()
            {

                BrowserId = Guid.Empty.ToString(),
                BoardName = source.PlaceName,
                AllowOthersToPostFliers = true,
                RequireApprovalOfPostedFliers = false,
                BoardTypeEnum = BoardTypeEnum.VenueBoard,
                SourceInformation = source,
                FlierIdToAddOnCreate = flierId
            };

            _commandBus.Send(createBoardCommand);

        }
    }
}