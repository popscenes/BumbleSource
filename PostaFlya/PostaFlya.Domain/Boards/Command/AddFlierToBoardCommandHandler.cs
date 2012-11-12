using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using PostaFlya.Domain.Boards.Event;
using PostaFlya.Domain.Flier;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Boards.Command
{
    internal class AddFlierToBoardCommandHandler : CommandHandlerInterface<AddFlierToBoardCommand>
    {

        private readonly GenericRepositoryInterface _repository;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly DomainEventPublishServiceInterface _domainEventPublishService;


        public AddFlierToBoardCommandHandler(GenericRepositoryInterface repository
            , GenericQueryServiceInterface queryService
            , UnitOfWorkFactoryInterface unitOfWorkFactory, DomainEventPublishServiceInterface domainEventPublishService)
        {
            _repository = repository;
            _queryService = queryService;
            _unitOfWorkFactory = unitOfWorkFactory;
            _domainEventPublishService = domainEventPublishService;
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

            var boardFliers = new List<BoardFlierModifiedEvent>();
            var unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new[] { _repository });
            using (unitOfWork)
            {
                foreach (var flierBoards in command.BoardFliers.ToLookup(bf => bf.FlierId, bf => bf.AggregateId))
                {
                    var flier = _queryService.FindById<Flier.Flier>(flierBoards.Key);
                    var boards = flierBoards.Where(id => flier.Boards == null || !flier.Boards.Contains(id)).ToList();
                    boardFliers.AddRange(
                        UpdateAddFlierToBoards(boards, flier, _queryService, _repository));


                }
            }

            if (!unitOfWork.Successful)
                return new MsgResponse("Adding Fliers To Boards Failed", true)
                        .AddCommandId(command);

            foreach (var boardFlier in boardFliers)
            {
                _domainEventPublishService.Publish(boardFlier);
            }

            return new MsgResponse("Added Fliers To Boards", false)
                .AddCommandId(command);

        }

        internal static List<BoardFlierModifiedEvent> UpdateAddFlierToBoards(List<string> boardIds
            , FlierInterface flier, GenericQueryServiceInterface queryService
            , GenericRepositoryInterface repository)
        {
            var ret = new List<BoardFlierModifiedEvent>();
            if (boardIds == null)
                return ret;
            foreach (var boardid in boardIds)
            {
                var board = queryService.FindById<Board>(boardid);
                if (board == null)
                {
                    continue;
                }

                var existing = queryService.FindById<BoardFlier>(flier.Id + board.Id);

                var boardFlier = new BoardFlier();
                if (board.BrowserId == flier.BrowserId)
                {
                    boardFlier.Status = BoardFlierStatus.Approved;
                }
                else if (board.AllowOthersToPostFliers)
                {
                    boardFlier.Status = board.RequireApprovalOfPostedFliers
                                            ? BoardFlierStatus.PendingApproval
                                            : BoardFlierStatus.Approved;
                }
                else
                {
                    continue;
                }

                boardFlier.Id = flier.Id + board.Id;
                boardFlier.FlierId = flier.Id;
                boardFlier.AggregateId = board.Id;
                boardFlier.DateAdded = DateTime.UtcNow;
                if(existing != null)
                    repository.UpdateEntity<BoardFlier>(existing.Id, bf => bf.CopyFieldsFrom(boardFlier));
                else
                    repository.Store(boardFlier);

                ret.Add(new BoardFlierModifiedEvent(){OrigState = existing, NewState = boardFlier});
            }

            repository.UpdateEntity<Flier.Flier>(flier.Id, update =>
            {
                if (update.Boards == null)
                    update.Boards = new List<string>();

                update.Boards.AddRange(ret
                    .Select(r => r.NewState.AggregateId)
                    .Where(id => !update.Boards.Contains(id)).ToList());
            });

            return ret;
        }

    }
}