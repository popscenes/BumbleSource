using System;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using Website.Azure.Common.Sql;

namespace Website.Azure.Common.Tests.Sql
{
    [TestFixture]
    public class SqlInitializerTests
    {
        [Test]
        public void SqlInitializerCreatesDatabaseIfNeeded()
        {
            const string databasename = "SqlInitializerTests";
            using (var initializer = new SqlInitializer())
            {
                initializer.CreateDb(databasename);

                Assert.IsTrue(initializer.HasDb(databasename));

                initializer.CreateDb(databasename);//second create doesn't cause error

                initializer.DeleteDb(databasename);

                Assert.IsFalse(initializer.HasDb(databasename));
            }
           
        }

        [Test]
        public void SqlInitializerDeleteNonExistentDatabaseOk()
        {
            const string databasename = "SqlInitializerTestsNe";
            using(var initializer = new SqlInitializer())
            {
                Assert.IsFalse(initializer.HasDb(databasename));

                initializer.DeleteDb(databasename);

                Assert.IsFalse(initializer.HasDb(databasename));
            }
        }

        [Test]
        public void SqlInitializerCreateTableFromTypeCreatesTable()
        {
            const string databasename = "SqlInitializerTests";
            using(var initializer = new SqlInitializer())
            {
                initializer.CreateDb(databasename);

                
                using (var connection = new SqlConnection(SqlExecute.GetConnectionStringFromConfig("DbConnectionString",databasename)))
                {
                    initializer.DeleteTable(typeof(SqlInitializerTestTable).Name, connection);

                    Assert.IsTrue(SqlInitializer.CreateTableFrom(typeof(SqlInitializerTestTable), connection));

                    Assert.IsTrue(initializer.DeleteTable(typeof(SqlInitializerTestTable).Name, connection));
                }

                initializer.DeleteDb(databasename);
            }
        }

        [Test]
        [TestCase(typeof(SqlInitializerTestFederatedTable))]
        public void SqlInitializerCreateTableFromTypeCreatesTableWithFederation(Type tableTyp)
        {
            const string databasename = "SqlInitializerTests";
            using (var initializer = new SqlInitializer())
            {
                initializer.CreateDb(databasename);

                using (var connection = new SqlConnection(SqlExecute.GetConnectionStringFromConfig("DbConnectionString", databasename)))
                {
                    initializer.DeleteTable(tableTyp.Name, connection);

                    Assert.IsTrue(SqlInitializer.CreateTableFrom(tableTyp, connection));

                    Assert.IsTrue(initializer.DeleteTable(tableTyp.Name, connection));

                    SqlInitializer.DeleteFederationFor(tableTyp, connection);
                }

                initializer.DeleteDb(databasename);
            }
        }

        [Test]
        public void SqlInitializerCreateTableFromTypeSpecifyingNameCreatesTable()
        {
            const string databasename = "SqlInitializerTests";
            using (var initializer = new SqlInitializer())
            {
                initializer.CreateDb(databasename);

                using (var connection = new SqlConnection(SqlExecute.GetConnectionStringFromConfig("DbConnectionString",databasename)))
                {
                    initializer.DeleteTable("BlahTable", connection);

                    Assert.IsTrue(SqlInitializer.CreateTableFrom(typeof(SqlInitializerTestTable), connection, "BlahTable"));

                    Assert.IsTrue(initializer.DeleteTable("BlahTable", connection));
                }

                initializer.DeleteDb(databasename);
            }
        }

        public static void SqlInitializerTestTable<TableType>(SqlInitializer initializer, string databasename)
        {

            initializer.CreateDb(databasename);

            using (var connection = new SqlConnection(SqlExecute.GetConnectionStringFromConfig("DbConnectionString",databasename)))
            {
                Assert.IsTrue(initializer.DeleteTable(typeof(TableType).Name, connection));
                Assert.IsTrue(SqlInitializer.CreateTableFrom(typeof(TableType), connection));
            }
        }

        public static void SqlInitializerDeleteTestTable<TableType>(SqlInitializer initializer, string databasename)
        {
            using (var connection = new SqlConnection(SqlExecute.GetConnectionStringFromConfig("DbConnectionString",databasename)))
            {
                Assert.IsTrue(initializer.DeleteTable(typeof(TableType).Name, connection));
                SqlInitializer.DeleteFederationFor(typeof(TableType), connection);
            }
        }
    }

    internal class SqlInitializerTestTable
    {

        public Guid Id { get; set; }

        public string Stringcol { get; set; }

        [PrimaryKey]
        public int Intcol { get; set; }

        public double Doublecol { get; set; }

        public DateTimeOffset Datecol { get; set; }

        public SqlXml XmlCol { get; set; }

        public long LongCol { get; set; }

        [NotNullable]
        [SpatialIndex]
        public SqlGeography Geography { get; set; }
    }

    internal class SqlInitializerTestFederatedTable
    {
        public Guid Id { get; set; }

        public string Stringcol { get; set; }

        [PrimaryKey]
        public int Intcol { get; set; }

        public double Doublecol { get; set; }

        public DateTimeOffset Datecol { get; set; }

        public SqlXml XmlCol { get; set; }

        [FederationCol(FederationName = "TestFederation", DistributionName = "long_col")]
        public long LongCol { get; set; }

        [NotNullable]
        [SpatialIndex]
        public SqlGeography Geography { get; set; }
    }
}
