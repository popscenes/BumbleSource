using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Xml.Linq;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using Website.Azure.Common.Sql;
using Website.Azure.Common.Sql.Infrastructure;
using Website.Test.Common;

namespace Website.Azure.Common.Tests.Sql
{
    [TestFixture]
    public class SqlExecuteTests
    {
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

        //[Test]
        public void SqlExecuteQueryReturnEnumerableOfRecordTypesForFederated()
        {

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
                    //SqlExecute.InsertOrUpdate(insert, conn);
                }
            }

            using (var conn = new SqlConnection(SqlExecute.GetConnectionStringFromConfig("DbConnectionString", "SqlInitializerTests")))
            {
                const string sqlCmd = "select * from SqlInitializerTestFederatedTable";

                var ret = SqlExecute.Query<SqlInitializerTestFederatedTable>(sqlCmd, conn, new object[]{0L}, null);
                AssertUtil.Count(5, ret);
                foreach (var sqlInitializerTestTable in ret)
                {
                    var ins = inserted.SingleOrDefault(r => r.Id == sqlInitializerTestTable.Id);
                    Assert.IsNotNull(ins);
                    AssertRecord(ins, sqlInitializerTestTable);
                }
            }
        }
    }
}
