using System;
using PostaFlya.Domain.Boards.Query;
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

        public CreateBoardCommandHandler(GenericRepositoryInterface boardRepository, GenericQueryServiceInterface boardQueryService, UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _boardRepository = boardRepository;
            _boardQueryService = boardQueryService;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(CreateBoardCommand command)
        {
            var newBoard = new Board()
                {
                    BrowserId = command.BrowserId,
                    Id = Guid.NewGuid().ToString(),
                    FriendlyId = command.BoardName,
                    Location = command.Location,
                    RequireApprovalOfPostedFliers = command.RequireApprovalOfPostedFliers,
                    AllowOthersToPostFliers = command.AllowOthersToPostFliers
                };

            var unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new object[] { _boardRepository, _boardQueryService });
            using (unitOfWork)
            {
                newBoard.FriendlyId = _boardQueryService.FindFreeFriendlyId(newBoard);
                _boardRepository.Store(newBoard);
            }

            if (!unitOfWork.Successful)
                return new MsgResponse("Board Creation Failed", true)
                        .AddCommandId(command);

            return new MsgResponse("Board Create", false)
                .AddEntityId(newBoard.Id)
                .AddCommandId(command);
        }
    }
}