using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using MbUnit.Framework;
using Microsoft.SqlServer.Types;
using Website.Azure.Common.Sql;

namespace Website.Azure.Common.Tests.Sql
{
    [TestFixture]
    public class SqlExecuteTests
    {
        [Test]
        public void SqlExecuteInsertsEntryForRecordClass()
        {
            const string databasename = "SqlInitializerTests";
            using (var initializer = new SqlInitializer())
            {
                SqlInitializerTests.SqlInitializerTestTable<SqlInitializerTestTable>(initializer, databasename);
            }

             
            var insert = new SqlInitializerTestTable()
                             {
                                 Id = Guid.NewGuid(),
                                 Geography = SqlGeography.Point(80, 80, SqlExecute.Srid),
                                 Datecol = DateTime.UtcNow.AddDays(-10),
                                 Doublecol = 123.0,
                                 Stringcol = "hello",
                                 XmlCol = new XElement("yo").ToSql(),
                                 Intcol = 3,
                                 LongCol = 30000000000L
                             };

            using(var conn = new SqlConnection(SqlExecute.GetConnectionStringFromConfig("DbConnectionString","SqlInitializerTests")))
            {
                Assert.IsTrue(SqlExecute.InsertOrUpdate(insert, conn));

                AssertGetRecord(insert, conn);
                
                Assert.IsTrue(SqlExecute.Delete(insert, conn));

                var ret = new SqlInitializerTestTable() { Intcol = insert.Intcol };
                Assert.IsFalse(SqlExecute.Get(ret, conn));
            }

            using (var initializer = new SqlInitializer())
            {
                SqlInitializerTests.SqlInitializerDeleteTestTable<SqlInitializerTestTable>(initializer, databasename);
            }
        }

        private static void AssertGetRecord(SqlInitializerTestTable insert, SqlConnection conn)
        {
            var ret = new SqlInitializerTestTable() {Intcol = insert.Intcol};
            Assert.IsTrue(SqlExecute.Get(ret, conn));
            AssertRecord(insert, ret);
        }

        private static void AssertRecord(SqlInitializerTestTable insert, SqlInitializerTestTable ret)
        {
            Assert.AreEqual(insert.Stringcol, ret.Stringcol);
            Assert.AreEqual(insert.Datecol, ret.Datecol);
            Assert.AreEqual(insert.Doublecol, ret.Doublecol);
            Assert.IsTrue(insert.Geography.STEquals(ret.Geography).IsTrue);
            Assert.AreEqual(insert.Id, ret.Id);
            Assert.AreEqual(insert.XmlCol.Value, ret.XmlCol.Value);
            Assert.AreEqual(insert.LongCol, ret.LongCol);
        }

        private static void AssertRecord(SqlInitializerTestFederatedTable insert, SqlInitializerTestFederatedTable ret)
        {
            Assert.AreEqual(insert.Stringcol, ret.Stringcol);
            Assert.AreEqual(insert.Datecol, ret.Datecol);
            Assert.AreEqual(insert.Doublecol, ret.Doublecol);
            Assert.IsTrue(insert.Geography.STEquals(ret.Geography).IsTrue);
            Assert.AreEqual(insert.Id, ret.Id);
            Assert.AreEqual(insert.XmlCol.Value, ret.XmlCol.Value);
            Assert.AreEqual(insert.LongCol, ret.LongCol);
        }

        [Test]
        public void SqlExecuteQueryReturnEnumerableOfRecordTypes()
        {
            const string databasename = "SqlInitializerTests";
            using (var initializer = new SqlInitializer())
            {
                SqlInitializerTests.SqlInitializerTestTable<SqlInitializerTestTable>(initializer, databasename);
            }

            var inserted = new List<SqlInitializerTestTable>();
            using (var conn = new SqlConnection(SqlExecute.GetConnectionStringFromConfig("DbConnectionString","SqlInitializerTests")))
            {
                for (int i = 0; i < 20; i++)
                {
                    var insert = new SqlInitializerTestTable()
                                     {
                                         Id = Guid.NewGuid(),
                                         Geography = SqlGeography.Point(50+i, 50+i, SqlExecute.Srid),
                                         Datecol = DateTime.UtcNow.AddDays(-(10 + i)),
                                         Doublecol = 123.0 + i,
                                         Stringcol = "hello" + i,
                                         XmlCol = new XElement("yo" + i).ToSql(),
                                         Intcol = 100 + i
                                     };
                    inserted.Add(insert);
                    SqlExecute.InsertOrUpdate(insert, conn);
                }
            }

            using (var conn = new SqlConnection(SqlExecute.GetConnectionStringFromConfig("DbConnectionString","SqlInitializerTests")))
            {
                const string sqlCmd = "select * from SqlInitializerTestTable";

                var ret = SqlExecute.Query<SqlInitializerTestTable>(sqlCmd, conn);
                Assert.Count(20, ret);
                foreach (var sqlInitializerTestTable in ret)
                {
                    var ins = inserted.SingleOrDefault(r => r.Id == sqlInitializerTestTable.Id);
                    Assert.IsNotNull(ins);
                    AssertRecord(ins, sqlInitializerTestTable);
                }
            }

            using (var initializer = new SqlInitializer())
            {
                SqlInitializerTests.SqlInitializerDeleteTestTable<SqlInitializerTestTable>(initializer, databasename);
            }
        }


        [Test]
        public void SqlExecuteQueryReturnEnumerableOfRecordTypesForFederated()
        {
            const string databasename = "SqlInitializerTests";
            using (var initializer = new SqlInitializer())
            {
                SqlInitializerTests.SqlInitializerTestTable<SqlInitializerTestFederatedTable>(initializer, databasename);
            }

            var inserted = new List<SqlInitializerTestFederatedTable>();
            using (var conn = new SqlConnection(SqlExecute.GetConnectionStringFromConfig("DbConnectionString", "SqlInitializerTests")))
            {
                for (int i = 0; i < 5; i++)
                {
                    var insert = new SqlInitializerTestFederatedTable()
                    {
                        Id = Guid.NewGuid(),
                        Geography = SqlGeography.Point(50 + i, 50 + i, SqlExecute.Srid),
                        Datecol = DateTime.UtcNow.AddDays(-(10 + i)),
                        Doublecol = 123.0 + i,
                        Stringcol = "hello" + i,
                        XmlCol = new XElement("yo" + i).ToSql(),
                        Intcol = 100 + i
                    };
                    inserted.Add(insert);
                    SqlExecute.InsertOrUpdate(insert, conn);
                }
            }

            using (var conn = new SqlConnection(SqlExecute.GetConnectionStringFromConfig("DbConnectionString", "SqlInitializerTests")))
            {
                const string sqlCmd = "select * from SqlInitializerTestFederatedTable";

                var ret = SqlExecute.Query<SqlInitializerTestFederatedTable>(sqlCmd, conn, new object[]{0}, null);
                Assert.Count(5, ret);
                foreach (var sqlInitializerTestTable in ret)
                {
                    var ins = inserted.SingleOrDefault(r => r.Id == sqlInitializerTestTable.Id);
                    Assert.IsNotNull(ins);
                    AssertRecord(ins, sqlInitializerTestTable);
                }
            }

            using (var initializer = new SqlInitializer())
            {
                SqlInitializerTests.SqlInitializerDeleteTestTable<SqlInitializerTestFederatedTable>(initializer, databasename);
            }
        }
    }
}
