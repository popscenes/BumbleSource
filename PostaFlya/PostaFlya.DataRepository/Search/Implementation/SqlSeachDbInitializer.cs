using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Ninject;
using PostaFlya.DataRepository.Search.SearchRecord;
using Website.Azure.Common.Binding;
using Website.Azure.Common.Sql;
using Website.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Binding;
using PostaFlya.Domain.Flier;

namespace PostaFlya.DataRepository.Search.Implementation
{
    internal class SqlSeachDbInitializer
    {
        private readonly string _masterConnection;
        private readonly string _searchDbConnectionString;

        public SqlSeachDbInitializer([SqlMasterDbConnectionString]string masterDb
            , [SqlSearchConnectionString]string searchDbConnectionString)
        {
            _masterConnection = masterDb;
            _searchDbConnectionString = searchDbConnectionString;
        }

        public void Initialize()
        {
            using (var init = new SqlInitializer(_masterConnection))
            {
                var searchDb = new SqlConnectionStringBuilder(_searchDbConnectionString);
                init.CreateDb(searchDb.InitialCatalog);
            }

            using (var newConn = new SqlConnection(_searchDbConnectionString))
            {
                //todo use reflection and interface to dynamically create all tables from 
                //an assembly cbfd right now
                SqlInitializer.CreateTableFrom(typeof (FlierSearchRecord), newConn);
                SqlInitializer.CreateTableFrom(typeof(BoardFlierLocationSearchRecord), newConn);
                SqlInitializer.CreateTableFrom(typeof(BoardSearchRecord), newConn); 
                SqlInitializer.CreateTableFrom(typeof(BoardFlierSearchRecord), newConn);


//not needed
//                var res = SqlExecute
//                    .Query<CountResult>("select Count(*) as Count from FlierSearchRecord", newConn)
//                    .SingleOrDefault();
//                if (res == null || res.Count == 0)
//                {
//                    foreach (
//                        var flier in
//                            _tableContext.PerformQuery<FlierTableEntry>().Select(
//                                ts => ts.CreateEntityCopy<Domain.Flier.Flier, FlierInterface>()).Distinct())
//                    {
//                        SqlExecute.InsertOrUpdate(flier.ToSearchRecord(), newConn);
//                    }
//
//                }
            }
        }

        public void DeleteAll()
        {
            using (var newConn = new SqlConnection(_searchDbConnectionString))
            {
                SqlExecute.ExecuteCommand("delete from FlierSearchRecord", newConn);
                SqlExecute.ExecuteCommand("delete from BoardFlierSearchRecord", newConn);
                SqlExecute.ExecuteCommand("delete from BoardFlierLocationSearchRecord", newConn);
                SqlExecute.ExecuteCommand("delete from BoardSearchRecord", newConn);
            }
        }
    }
}
