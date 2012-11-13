using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using PostaFlya.DataRepository.Binding;
using PostaFlya.DataRepository.Search.Command;
using PostaFlya.DataRepository.Search.Services;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Event;
using PostaFlya.Domain.Flier.Event;
using Website.Application.Binding;
using Website.Azure.Common.Sql;
using Website.Infrastructure.Command;
using Website.Infrastructure.Publish;
using Website.Infrastructure.Query;

namespace PostaFlya.DataRepository.Search.Event
{
    public class SqlFlierIndexService : 
        SubscriptionInterface<FlierModifiedEvent>
        , SubscriptionInterface<BoardFlierModifiedEvent>
        , SubscriptionInterface<BoardModifiedEvent>
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
//                command.SearchRecord = publish.NewState.ToSearchRecord();
//                command.UpdateOrDelete = true;
//                return (bool) _commandBus.Send(command);
//            }
//                
//            if(publish.OrigState != null)
//            {
//                command.SearchRecord = publish.OrigState.ToSearchRecord();
//                command.UpdateOrDelete = false;
//                return (bool) _commandBus.Send(command);
//            }
//
//            return false;
//        }

        
        public bool Publish(FlierModifiedEvent publish)
        {
            if (publish.OrigState != null &&
                (publish.NewState == null || !publish.NewState.Location.Equals(publish.OrigState.Location)))
            {
                var searchRecord = publish.OrigState.ToSearchRecord();
                SqlExecute.Delete(searchRecord, _connection);
                if (publish.OrigState.Boards != null)
                {
                    foreach (var searcRec in publish.OrigState.Boards
                        .Select(id => new BoardFlier() { FlierId = publish.NewState.Id, AggregateId = id })
                        .Select(boardFlier => _queryService.FindById<BoardFlier>(boardFlier.GetIdFor()))
                        .Select(retrieved => retrieved.ToSearchRecord(publish.NewState.Location.GetShardId())))
                    {
                        SqlExecute.Delete(searchRecord, _connection);
                    }    
                }
            }

            if (publish.NewState != null)
            {
                var searchRecord = publish.NewState.ToSearchRecord();
                SqlExecute.InsertOrUpdate(searchRecord, _connection);
                if (publish.NewState.Boards != null)
                {
                    foreach (var searcRec in publish.NewState.Boards
                        .Select(id => new BoardFlier() { FlierId = publish.NewState.Id, AggregateId = id })
                        .Select(boardFlier => _queryService.FindById<BoardFlier>(boardFlier.GetIdFor()))
                        .Select(retrieved => retrieved.ToSearchRecord(publish.NewState.Location.GetShardId())))
                    {
                        SqlExecute.InsertOrUpdate(searchRecord, _connection);
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

        public bool IsEnabled { get; private set; }
        public string Description { get; private set; }
        public string Name { get; private set; }
    }
}
