using System;
using System.Data;
using System.Linq;
using AutoMapper;
using NLog;
using NUnit.Framework;
using Website.Azure.Common.Sql;
using Website.Azure.Common.Sql.Infrastructure;
using Website.Infrastructure.Execution;

namespace Website.Azure.Common.Tests.Sql
{
    public class ArmTestBase
    {

        internal ExecutionEnvironment Environment { get; set; }

        protected readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [SetUp]
        public void Init()
        {
            Environment = new ExecutionEnvironment()
                {
                    Id = "Default",
                    ConnectionInfo = SqlExecute.GetConnectionStringFromConfig("DbConnectionString", "SqlInitializerTests") 
                };

            OrdersMapping.RecreateSchema(Environment);
        }

        [TearDown]
        public void Dispose()
        {

        }

    }


}