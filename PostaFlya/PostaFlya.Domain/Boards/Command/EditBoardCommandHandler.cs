using PostaFlya.Domain.Boards.Query;
using Website.Domain.Browser;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Command
{
    public class EditBoardCommandHandler : MessageHandlerInterface<EditBoardCommand>
    {
        private readonly GenericRepositoryInterface _boardRepository;
        private readonly GenericQueryServiceInterface _boardQueryService;
        private readonly QueryChannelInterface _queryChannel;

        public EditBoardCommandHandler(GenericRepositoryInterface boardRepository
                                       , GenericQueryServiceInterface boardQueryService
                                       , QueryChannelInterface queryChannel)
        {
            _boardRepository = boardRepository;
            _boardQueryService = boardQueryService;
            _queryChannel = queryChannel;
        }

        public void Handle(EditBoardCommand command)
        {
            var boardExist = _boardQueryService.FindById<Board>(command.Id);
            var brows = _boardQueryService.FindById<PostaFlya.Domain.Browser.Browser>(command.BrowserId);

            if(boardExist == null || (boardExist.BrowserId != command.BrowserId && !brows.HasRole(Role.Admin)))
                return;

            if (!brows.HasRole(Role.Admin))
                command.Status = boardExist.Status;

            _boardRepository.UpdateEntity<Board>(command.Id, board =>
                {
                    board.Status = command.Status;

                    if (!string.IsNullOrWhiteSpace(command.BoardName))
                    {
                        board.FriendlyId = command.BoardName;
                        board.FriendlyId = _queryChannel.FindFreeFriendlyId(board);
                    }

                    if (!string.IsNullOrWhiteSpace(command.Description))
                    {
                        board.Description = command.Description;
                    }

                    if (!string.IsNullOrWhiteSpace(command.LogoImageId))
                    {
                        board.ImageId = command.LogoImageId;
                    }

                } );
        }
    }
}