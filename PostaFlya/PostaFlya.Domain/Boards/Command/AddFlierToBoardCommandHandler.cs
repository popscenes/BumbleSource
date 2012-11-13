using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using PostaFlya.Domain.Boards.Event;
using PostaFlya.Domain.Flier;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
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
                    var boards = flierBoards.Where(id => flier.Boards == null || !flier.Boards.Contains(id));
                    boardFliers.AddRange(
                        UpdateAddFlierToBoards(new HashSet<string>(boards), flier, _queryService, _repository));
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

        internal static List<BoardFlierModifiedEvent> UpdateAddFlierToBoards(HashSet<string> boardIds
            , Flier.Flier flier, GenericQueryServiceInterface queryService
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

                var existing = new BoardFlier(){FlierId = flier.Id, AggregateId = board.Id};
                existing = queryService.FindById<BoardFlier>(existing.GetIdFor());

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

                
                boardFlier.FlierId = flier.Id;
                boardFlier.AggregateId = board.Id;
                boardFlier.DateAdded = DateTime.UtcNow;
                boardFlier.Id = boardFlier.GetIdFor();
                if(existing != null)
                    repository.UpdateEntity<BoardFlier>(existing.Id, bf => bf.CopyFieldsFrom(boardFlier));
                else
                    repository.Store(boardFlier);

                var modEvent = new BoardFlierModifiedEvent {NewState = boardFlier, OrigState = existing};
                ret.Add(modEvent);
            }

            repository.UpdateEntity<Flier.Flier>(flier.Id, update =>
            {
                if (update.Boards == null)
                    update.Boards = new HashSet<string>();

                update.Boards.UnionWith(ret.Select(r => r.NewState.AggregateId));
                flier.Boards.UnionWith(update.Boards);
            });

            return ret;
        }

    }
}