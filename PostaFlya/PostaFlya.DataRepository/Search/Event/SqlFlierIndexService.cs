﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using PostaFlya.DataRepository.Binding;
using PostaFlya.DataRepository.Search.Command;
using PostaFlya.DataRepository.Search.Services;
using PostaFlya.Domain.Flier.Event;
using Website.Application.Binding;
using Website.Azure.Common.Sql;
using Website.Infrastructure.Command;
using Website.Infrastructure.Publish;

namespace PostaFlya.DataRepository.Search.Event
{
    public class SqlFlierIndexService : SubscriptionInterface<FlierModifiedEvent>
    {
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
        public SqlFlierIndexService([SqlSearchConnectionString]string searchDbConnectionString)
        {
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
            if (publish.NewState != null)
            {
                var searchRecord = publish.NewState.ToSearchRecord();
                SqlExecute.InsertOrUpdate(searchRecord, _connection);
            }
                        
            if(publish.OrigState != null)
            {
                var searchRecord = publish.OrigState.ToSearchRecord();
                SqlExecute.Delete(searchRecord, _connection);
            }
        
            return false;
        }

        public bool IsEnabled { get; private set; }
        public string Description { get; private set; }
        public string Name { get; private set; }
    }
}
