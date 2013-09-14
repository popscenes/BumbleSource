//using System.Data.SqlClient;
//using System.Linq;
//using PostaFlya.DataRepository.Binding;
//using PostaFlya.DataRepository.Search.SearchRecord;
//using PostaFlya.Domain.Boards;
//using PostaFlya.Domain.Flier;
//using PostaFlya.Domain.Flier.Query;
//using Website.Azure.Common.Sql;
//using Website.Domain.Claims;
//using Website.Domain.Comments;
//using Website.Infrastructure.Domain;
//using Website.Infrastructure.Messaging;
//using Website.Infrastructure.Query;
//
//namespace PostaFlya.DataRepository.Search.Event
//{
//    public class SqlFlierIndexService : 
//        HandleEventInterface<EntityModifiedEvent<Flier>>
//        , HandleEventInterface<EntityModifiedEvent<Board>>
//        , HandleEventInterface<EntityModifiedEvent<Claim>>
//        , HandleEventInterface<EntityModifiedEvent<Comment>>
//    {
//        private readonly GenericQueryServiceInterface _queryService;
//        private readonly QueryChannelInterface _queryChannel;
//
//
//        private readonly SqlConnection _connection;
//        public SqlFlierIndexService([SqlSearchConnectionString]string searchDbConnectionString
//            , GenericQueryServiceInterface queryService, QueryChannelInterface queryChannel)
//        {
//            _queryService = queryService;
//            _queryChannel = queryChannel;
//            _connection = new SqlConnection(searchDbConnectionString);
//        }
//        
//        public bool Handle(EntityModifiedEvent<Flier> @event)
//        {
//
//            SqlExecute.DeleteBy(new FlierSearchRecord()
//                {
//                    Id = @event.Entity.Id
//                }, _connection, record => record.Id);
//
//            SqlExecute.DeleteBy(new FlierDateSearchRecord()
//            {
//                Id = @event.Entity.Id
//            }, _connection, record => record.Id);
//
//            if (!@event.IsDeleted && @event.Entity.Status == FlierStatus.Active)
//            {
//                var venueBoard = _queryChannel.Query(new GetFlyerVenueBoardQuery() {FlyerId = @event.Entity.Id}, (Board)null);
//
//                var searchRecords = @event.Entity.ToSearchRecords(venueBoard).ToList();
//                SqlExecute.InsertOrUpdateAll(searchRecords, _connection);
//
//                var eventDatesRecs = @event.Entity.ToDateSearchRecords(searchRecords);
//                SqlExecute.DeleteBy(searchRecords.Select(record => new FlierDateSearchRecord() { LocationShard = record.LocationShard, Id = record.Id })
//                    , _connection, e => e.Id, e => e.LocationShard);
//                SqlExecute.InsertOrUpdateAll(eventDatesRecs, _connection);
//            }
//
//            AddBoardFlyerRecs(@event);
//
//            return true;
//        }
//
//        private bool AddBoardFlyerRecs(EntityModifiedEventInterface<Flier> @event)
//        {
//
//            SqlExecute.DeleteBy(new BoardFlierSearchRecord()
//                {
//                    FlierId = @event.Entity.Id
//                }, _connection, record => record.FlierId);
//            SqlExecute.DeleteBy(new BoardFlierDateSearchRecord()
//                {
//                    FlyerId = @event.Entity.Id
//                }, _connection, record => record.FlyerId);
//
//            if (!@event.IsDeleted && @event.Entity.Boards != null && @event.Entity.Boards.Any())
//            {
//                var flier = _queryService.FindById<Domain.Flier.Flier>(@event.Entity.Id);
//                if (flier == null)
//                    return false;
//                var venueBoard = _queryChannel.Query(new GetFlyerVenueBoardQuery() {FlyerId = flier.Id}, (Board) null);
//
//                foreach (var searchRecord in flier.Boards.Select(board => board.ToSearchRecord(flier, venueBoard)))
//                {
//                    SqlExecute.InsertOrUpdate(searchRecord, _connection);
//                    SqlExecute.DeleteBy(new BoardFlierDateSearchRecord()
//                        {
//                            BoardId = searchRecord.BoardId,
//                            Id = searchRecord.Id
//                        }, _connection, record => record.BoardId, record => record.Id);
//
//                    SqlExecute.InsertOrUpdateAll(searchRecord.ToBoardDateSearchRecords(flier), _connection);
//                }
//
//
//            }
//
//            return true;
//
//        }
//
//        public bool Handle(EntityModifiedEvent<Board> @event)
//        {
//            SqlExecute.DeleteBy(new BoardSearchRecord() { Id = @event.Entity.Id }
//                , _connection, record => record.Id);
//
//            if (!@event.IsDeleted && @event.Entity.InformationSources != null 
//                && @event.Entity.InformationSources.First().Address != null 
//                && @event.Entity.InformationSources.First().Address.IsValid 
//                && @event.Entity.BoardTypeEnum != BoardTypeEnum.InterestBoard)
//            {
//                var searchRecord = @event.Entity.ToSearchRecord();
//                SqlExecute.InsertOrUpdate(searchRecord, _connection);
//            }
//            return true;
//
//        }
//
//        private bool ProcessAggregateMemberChanged(AggregateInterface newState)
//        {
//            if (newState == null || newState.AggregateTypeTag == typeof(Flier).Name)
//                return false;
//
//            var flier = _queryService.FindById<Domain.Flier.Flier>(newState.AggregateId);
//            if (flier == null)
//                return false;
//
//            var venueBoard = _queryChannel.Query(new GetFlyerVenueBoardQuery() { FlyerId = flier.Id }, (Board)null);
//
//            var searchRecord = flier.ToSearchRecords(venueBoard);
//            SqlExecute.InsertOrUpdateAll(searchRecord, _connection);
//            return true;
//        }
//        public bool Handle(EntityModifiedEvent<Claim> @event)
//        {
//            return ProcessAggregateMemberChanged(@event.Entity);
//        }
//
//        public bool Handle(EntityModifiedEvent<Comment> @event)
//        {
//            return ProcessAggregateMemberChanged(@event.Entity);
//        }
//    }
//}
