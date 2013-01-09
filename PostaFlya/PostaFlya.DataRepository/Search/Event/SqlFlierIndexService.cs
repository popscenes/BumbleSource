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
        SubscriptionInterface<FlierModifiedEvent>
        , SubscriptionInterface<BoardFlierModifiedEvent>
        , SubscriptionInterface<BoardModifiedEvent>
        , SubscriptionInterface<ClaimEvent>
        , SubscriptionInterface<CommentEvent>
    {
        private readonly GenericQueryServiceInterface _queryService;
        //if we wanna push this to a worker....not really needed atm.
//        private readonly CommandBusInterface _commandBus;

//        public SqlFlierIndexService([WorkerCommandBus]CommandBusInterface commandBus)
//        {
//            _commandBus = commandBus;
//            IsEnabled = true;
//            Name = "SqlFlierIndexService";
//            Description = "Indexes Fliers in SQL to allow for efficient searching";
//        }

        private readonly SqlConnection _connection;
        public SqlFlierIndexService([SqlSearchConnectionString]string searchDbConnectionString
            , GenericQueryServiceInterface queryService)
        {
            _queryService = queryService;
            _connection = new SqlConnection(searchDbConnectionString);
            IsEnabled = true;
            Name = "SqlFlierIndexService";
            Description = "Indexes Fliers in SQL to allow for efficient searching";
        }

        //if we wanna push this to a worker....not really needed atm.
//        public bool Publish(FlierModifiedEvent publish)
//        {
//            var command = new ReindexFlierCommand();
//            if (publish.NewState != null)
//            {
//                command.SearchRecord = publish.NewState.ToSearchRecords();
//                command.UpdateOrDelete = true;
//                return (bool) _commandBus.Send(command);
//            }
//                
//            if(publish.OrigState != null)
//            {
//                command.SearchRecord = publish.OrigState.ToSearchRecords();
//                command.UpdateOrDelete = false;
//                return (bool) _commandBus.Send(command);
//            }
//
//            return false;
//        }

        
        public bool Publish(FlierModifiedEvent publish)
        {
            if (publish.OrigState != null &&
                (publish.NewState == null || 
                !publish.NewState.Location.Equals(publish.OrigState.Location) ||
                publish.NewState.LocationRadius != publish.OrigState.LocationRadius ||
                (publish.OrigState.Status == FlierStatus.Active && publish.NewState.Status != FlierStatus.Active))
             )
            {
                var searchRecords = publish.OrigState.ToSearchRecords().ToList();
                SqlExecute.DeleteAll(searchRecords, _connection);
                if (publish.OrigState.Boards != null)
                {
                    foreach (var boardsForShard in 
                        searchRecords.Select(flierSearchRecord => publish.OrigState.Boards
                            .Select(id => new BoardFlier() { FlierId = publish.OrigState.Id, AggregateId = id })
                            .Select(boardFlier => _queryService.FindById<BoardFlier>(boardFlier.GetIdFor()))
                            .Select(
                                retrieved =>
                                retrieved.ToSearchRecord(flierSearchRecord.LocationShard)).ToList()))
                    {
                        SqlExecute.DeleteAll(boardsForShard, _connection);
                    }
                        
                }
            }

            if (publish.NewState != null && publish.NewState.Status == FlierStatus.Active)
            {
                var searchRecords = publish.NewState.ToSearchRecords().ToList();
                SqlExecute.InsertOrUpdateAll(searchRecords, _connection);
                if (publish.NewState.Boards != null)
                {
                    foreach (var boardsForShard in
                        searchRecords.Select(flierSearchRecord => publish.NewState.Boards
                            .Select(id => new BoardFlier() { FlierId = publish.NewState.Id, AggregateId = id })
                            .Select(boardFlier => _queryService.FindById<BoardFlier>(boardFlier.GetIdFor()))
                            .Select(
                                retrieved =>
                                retrieved.ToSearchRecord(flierSearchRecord.LocationShard)).ToList()))
                    {
                        SqlExecute.InsertOrUpdateAll(boardsForShard, _connection);
                    }

                }
            }

            return (publish.OrigState != null) || (publish.NewState != null);
        }

        public bool Publish(BoardFlierModifiedEvent publish)
        {
            if (publish.OrigState != null && publish.NewState == null)
            {
                var flier = _queryService.FindById<Domain.Flier.Flier>(publish.OrigState.FlierId);
                var searchRecord = publish.OrigState.ToSearchRecord(flier);
                SqlExecute.Delete(searchRecord, _connection);
            }

            if (publish.NewState != null)
            {
                var flier = _queryService.FindById<Domain.Flier.Flier>(publish.NewState.FlierId);
                if (flier == null)
                    return false;
                var searchRecord = publish.NewState.ToSearchRecord(flier);
                SqlExecute.InsertOrUpdate(searchRecord, _connection);
            }

            return (publish.OrigState != null) || (publish.NewState != null);

        }

        public bool Publish(BoardModifiedEvent publish)
        {
            if (publish.OrigState != null &&
                (publish.NewState == null || !publish.NewState.Location.Equals(publish.OrigState.Location)))
            {
                var searchRecord = publish.OrigState.ToSearchRecord();
                SqlExecute.Delete(searchRecord, _connection);
            }

            if (publish.NewState != null)
            {
                var searchRecord = publish.NewState.ToSearchRecord();
                SqlExecute.InsertOrUpdate(searchRecord, _connection);
            }
            return (publish.OrigState != null) || (publish.NewState != null);

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
        public bool Publish(ClaimEvent publish)
        {
            return ProcessAggregateMemberChanged(publish.NewState);
        }

        public bool Publish(CommentEvent publish)
        {
            return ProcessAggregateMemberChanged(publish.NewState);
        }

        public bool IsEnabled { get; private set; }
        public string Description { get; private set; }
        public string Name { get; private set; }
    }
}
