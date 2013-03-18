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
            const string databasename = "SqlInitializerTestsCreateDelete";
            using (var initializer = new SqlInitializer())
            {
                initializer.CreateDb(databasename);

                Assert.IsTrue(initializer.HasDb(databasename));

                initializer.CreateDb(databasename);//second create doesn't cause error
            }

            using (var initializer = new SqlInitializer())
            {
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
            }
        }

        [Test]
        [TestCase(typeof(SqlInitializerTestFederatedTable))]
        //[TestCase(typeof(SqlInitializerTestAnotherFederatedTableOnString))] strings not supported
        public void SqlInitializerCreateTableFromTypeCreatesTableWithFederation(Type tableTyp)
        {
            const string databasename = "SqlInitializerTests";
            using (var initializer = new SqlInitializer())
            {
                initializer.CreateDb(databasename);

                using (var connection = new SqlConnection(SqlExecute.GetConnectionStringFromConfig("DbConnectionString", databasename)))
                {
                    initializer.DeleteTableInContext(tableTyp, tableTyp.Name, connection);

                    Assert.IsTrue(SqlInitializer.CreateTableFrom(tableTyp, connection));

                    Assert.IsTrue(initializer.DeleteTable(tableTyp.Name, connection));

                    SqlInitializer.DeleteFederationFor(tableTyp, connection);
                }

            }
        }

        [Test]
        [TestCase(typeof(SqlInitializerTestFederatedTable), typeof(SqlInitializerTestAnotherFederatedTable))]
        public void SqlInitializerCreateFederationsForTwoDifferentTypes(Type tableTyp, Type tableTypTwo)
        {
            const string databasename = "SqlInitializerTests";
            using (var initializer = new SqlInitializer())
            {
                initializer.CreateDb(databasename);

                using (var connection = new SqlConnection(SqlExecute.GetConnectionStringFromConfig("DbConnectionString", databasename)))
                {
                    SqlInitializer.CreateFederationFor(tableTyp, connection);
                    SqlInitializer.CreateFederationFor(tableTypTwo, connection);

                    SqlInitializer.DeleteFederationFor(tableTyp, connection);
                    SqlInitializer.DeleteFederationFor(tableTypTwo, connection);
                }

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
                Assert.IsTrue(initializer.DeleteTableInContext(typeof(TableType), typeof(TableType).Name, connection));
                SqlInitializer.DeleteFederationFor(typeof(TableType), connection);
            }
        }

        [Test]
        public void SqlInitializerCreatesTableWithIndex()
        {
            const string databasename = "SqlInitializerTests";
            using (var initializer = new SqlInitializer())
            {
                initializer.CreateDb(databasename);


                using (var connection = new SqlConnection(SqlExecute.GetConnectionStringFromConfig("DbConnectionString", databasename)))
                {
                    initializer.DeleteTable(typeof(SqlInitializerTestIndexTable).Name, connection);

                    //test twice to make sure that indexes are checked before creating
                    Assert.IsTrue(SqlInitializer.CreateTableFrom(typeof(SqlInitializerTestIndexTable), connection));
                    Assert.IsTrue(SqlInitializer.CreateTableFrom(typeof(SqlInitializerTestIndexTable), connection));

                    Assert.IsTrue(initializer.DeleteTable(typeof(SqlInitializerTestIndexTable).Name, connection));
                }

            }
        }

        [Test]
        public void SqlInitializerCreatesFederatedReferenceTableWithIndex()
        {
            const string databasename = "SqlInitializerTests";
            using (var initializer = new SqlInitializer())
            {
                initializer.CreateDb(databasename);


                using (var connection = new SqlConnection(SqlExecute.GetConnectionStringFromConfig("DbConnectionString", databasename)))
                {
                    initializer.DeleteTable(typeof(SqlInitializerTestFedRefernceTableWithIndex).Name, connection);

                    Assert.IsTrue(SqlInitializer.CreateTableFrom(typeof(SqlInitializerTestFedRefernceTableWithIndex), connection));

                    Assert.IsTrue(initializer.DeleteTable(typeof(SqlInitializerTestFedRefernceTableWithIndex).Name, connection));
                    SqlInitializer.DeleteFederationFor(typeof(SqlInitializerTestFedRefernceTableWithIndex), connection);
                }


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


    internal class SqlInitializerTestFedRefernceTableWithIndex
    {
        [PrimaryKey]
        public string Stringcol { get; set; }

        [SqlIndex]
        public long LongCol { get; set; }

        [FederationCol(FederationName = "TestFederation", DistributionName = "long_col", IsReferenceTable = true)]        
        public long FedRefCol { get; set; }

    }

    internal class SqlInitializerTestIndexTable
    {
        [PrimaryKey]
        public string Stringcol { get; set; }

        [SqlIndex]
        public long LongCol { get; set; }

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

    internal class SqlInitializerTestAnotherFederatedTable
    {
        public Guid Id { get; set; }

        public string Stringcol { get; set; }

        [PrimaryKey]
        public int Intcol { get; set; }

        public double Doublecol { get; set; }

        public DateTimeOffset Datecol { get; set; }

        public SqlXml XmlCol { get; set; }

        [FederationCol(FederationName = "TestFederationAnother", DistributionName = "fed_col")]
        public Guid FedCol { get; set; }

        [NotNullable]
        [SpatialIndex]
        public SqlGeography Geography { get; set; }
    }

    internal class SqlInitializerTestAnotherFederatedTableOnString
    {
        public Guid Id { get; set; }

        [FederationCol(FederationName = "TestFederationAnotherString", DistributionName = "fed_col")]
        public string Stringcol { get; set; }

        [PrimaryKey]
        public int Intcol { get; set; }

        public double Doublecol { get; set; }

        public DateTimeOffset Datecol { get; set; }

        public SqlXml XmlCol { get; set; }

        
        public Guid FedCol { get; set; }

        [NotNullable]
        [SpatialIndex]
        public SqlGeography Geography { get; set; }
    }
}
