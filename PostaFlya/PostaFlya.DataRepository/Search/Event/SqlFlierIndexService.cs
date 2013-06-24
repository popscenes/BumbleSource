using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using PostaFlya.DataRepository.Binding;
using PostaFlya.DataRepository.Search.Command;
using PostaFlya.DataRepository.Search.SearchRecord;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Event;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Event;
using Website.Application.Binding;
using Website.Azure.Common.Sql;
using Website.Domain.Claims.Event;
using Website.Domain.Comments.Event;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Publish;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.Search.Event
{
    public class SqlFlierIndexService : 
        HandleEventInterface<FlierModifiedEvent>
        , HandleEventInterface<BoardModifiedEvent>
        , HandleEventInterface<ClaimEvent>
        , HandleEventInterface<CommentEvent>
    {
        private readonly GenericQueryServiceInterface _queryService;


        private readonly SqlConnection _connection;
        public SqlFlierIndexService([SqlSearchConnectionString]string searchDbConnectionString
            , GenericQueryServiceInterface queryService)
        {
            _queryService = queryService;
            _connection = new SqlConnection(searchDbConnectionString);
        }
        
        public bool Handle(FlierModifiedEvent @event)
        {
            if (@event.OrigState != null &&
                (@event.NewState == null ||
                !@event.NewState.Venue.Address.Equals(@event.OrigState.Venue.Address) ||
                @event.NewState.LocationRadius != @event.OrigState.LocationRadius ||
                @event.NewState.Status != FlierStatus.Active)
             )
            {
                var searchRecords = @event.OrigState.ToSearchRecords().ToList();
                SqlExecute.DeleteAll(searchRecords, _connection);
                SqlExecute.DeleteBy(searchRecords.Select(record => new FlierDateSearchRecord() { LocationShard = record.LocationShard, Id = record.Id })
                    , _connection, e => e.Id, e => e.LocationShard);
            }

            if (@event.NewState != null && @event.NewState.Status == FlierStatus.Active)
            {
                var searchRecords = @event.NewState.ToSearchRecords().ToList();
                SqlExecute.InsertOrUpdateAll(searchRecords, _connection);

                var eventDatesRecs = @event.NewState.ToDateSearchRecords(searchRecords);
                SqlExecute.DeleteBy(searchRecords.Select(record => new FlierDateSearchRecord() { LocationShard = record.LocationShard, Id = record.Id })
                    , _connection, e => e.Id, e => e.LocationShard);
                SqlExecute.InsertOrUpdateAll(eventDatesRecs, _connection);
            }

            AddBoardFlyerRecs(@event);

            return (@event.OrigState != null) || (@event.NewState != null);
        }

        private bool AddBoardFlyerRecs(EntityModifiedDomainEventInterface<Flier> @event)
        {
            if (@event.OrigState != null && @event.OrigState.Boards != null && @event.OrigState.Boards.Any())
            {
                var flier = _queryService.FindById<Domain.Flier.Flier>(@event.OrigState.Id);
                var boards = _queryService.FindByIds<Board>(@event.OrigState.Boards);
                foreach (var searchRecord in boards.Select(board => board.ToSearchRecord(flier)))
                {
                    SqlExecute.Delete(searchRecord, _connection);
                    SqlExecute.DeleteBy(new BoardFlierDateSearchRecord()
                        {
                            BoardId = searchRecord.BoardId,
                            Id = searchRecord.Id
                        }, _connection, record => record.BoardId, record => record.Id);
                }

            }

            if (@event.NewState != null && @event.NewState.Boards != null && @event.NewState.Boards.Any())
            {
                var flier = _queryService.FindById<Domain.Flier.Flier>(@event.NewState.Id);
                if (flier == null)
                    return false;
                var boards = _queryService.FindByIds<Board>(@event.NewState.Boards);

                foreach (var searchRecord in boards.Select(board => board.ToSearchRecord(flier)))
                {
                    SqlExecute.InsertOrUpdate(searchRecord, _connection);
                    SqlExecute.DeleteBy(new BoardFlierDateSearchRecord()
                        {
                            BoardId = searchRecord.BoardId,
                            Id = searchRecord.Id
                        }, _connection, record => record.BoardId, record => record.Id);

                    SqlExecute.InsertOrUpdateAll(searchRecord.ToBoardDateSearchRecords(flier), _connection);
                }


            }

            return (@event.OrigState != null) || (@event.NewState != null);

        }

        public bool Handle(BoardModifiedEvent @event)
        {
            
            if (@event.OrigState != null &&
                (@event.NewState.BoardTypeEnum == BoardTypeEnum.InterestBoard ||
                    @event.NewState == null || !@event.NewState.InformationSources.First().Address.Equals(@event.OrigState.InformationSources.First().Address)))
            {
                var searchRecord = @event.OrigState.ToSearchRecord();
                SqlExecute.Delete(searchRecord, _connection);
            }

            if (@event.NewState != null && @event.NewState.InformationSources  != null 
                && @event.NewState.InformationSources.First().Address != null 
                && @event.NewState.InformationSources.First().Address.IsValid 
                && @event.NewState.BoardTypeEnum != BoardTypeEnum.InterestBoard)
            {
                var searchRecord = @event.NewState.ToSearchRecord();
                SqlExecute.InsertOrUpdate(searchRecord, _connection);
            }
            return (@event.OrigState != null) || (@event.NewState != null);

        }

        private bool ProcessAggregateMemberChanged(AggregateInterface newState)
        {
            if (newState == null || newState.AggregateTypeTag == typeof(Flier).Name)
                return false;

            var flier = _queryService.FindById<Domain.Flier.Flier>(newState.AggregateId);
            if (flier == null)
                return false;

            var searchRecord = flier.ToSearchRecords();
            SqlExecute.InsertOrUpdateAll(searchRecord, _connection);
            return true;
        }
        public bool Handle(ClaimEvent @event)
        {
            return ProcessAggregateMemberChanged(@event.NewState);
        }

        public bool Handle(CommentEvent @event)
        {
            return ProcessAggregateMemberChanged(@event.NewState);
        }
    }
}
