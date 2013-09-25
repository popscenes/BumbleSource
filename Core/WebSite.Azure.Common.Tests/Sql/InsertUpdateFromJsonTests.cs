using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Dapper;
using FizzWare.NBuilder;
using NUnit.Framework;
using Newtonsoft.Json;
using Website.Azure.Common.Sql;
using Website.Azure.Common.Sql.Infrastructure;

namespace Website.Azure.Common.Tests.Sql
{
    [TestFixture]
    public class InsertUpdateFromJsonTests : ArmTestBase
    {

        [Test]
        public void Insert_IntoTable_From_Json()
        {
            var newOrder = new Order()
            {
                Id = "123",
                CustomerId = "234565",
                Items = new List<OrderLineItem>() { new OrderLineItem() { Quantity = 444 } }
            };

            var json = JsonConvert.SerializeObject(newOrder);
            var clr = newOrder.GetType().GetAssemblyQualifiedNameWithoutVer();

            Action<IDbConnection> update = db => db.UpdateOrInsertAggregateRootFromJson(json, clr);
            update.ExecuteInEnvironment(Environment);


            Func<IDbConnection, Order> query = db => db.GetAggregateRoot<Order>(newOrder.Id);
            var res = query.ExecuteInEnvironment(Environment);

            Assert.That(res.CustomerId, Is.EqualTo("234565"));
            Assert.That(res.Id, Is.EqualTo("123"));
        }


        [Test]
        public void Update_Table_From_Json()
        {
            var newOrder = new Order()
            {
                Id = "123",
                CustomerId = "234565",
                Items = new List<OrderLineItem>() { new OrderLineItem() { Quantity = 444 } }
            };

            Action<IDbConnection> insert = db => db.InsertAggregateRoot(newOrder);
            insert.ExecuteInEnvironment(Environment);

            Func<IDbConnection, Order> query = db => db.GetAggregateRoot<Order>(newOrder.Id);
            var res = query.ExecuteInEnvironment(Environment);

            Assert.That(res.CustomerId, Is.EqualTo("234565"));
            Assert.That(res.Id, Is.EqualTo("123"));

            newOrder.CustomerId = "222222";
            var json = JsonConvert.SerializeObject(newOrder);
            var clr = newOrder.GetType().GetAssemblyQualifiedNameWithoutVer();


            Action<IDbConnection> update = db => db.UpdateOrInsertAggregateRootFromJson(json, clr);
            update.ExecuteInEnvironment(Environment);

            res = query.ExecuteInEnvironment(Environment);

            Assert.That(res.CustomerId, Is.EqualTo("222222"));
            Assert.That(res.Id, Is.EqualTo("123"));
        }

        [Test]
        public void AggregateStorage_TableRows_With_Negative_Json_Hash_Rebuild_Mapping_Tables()
        {
            const int perfcount = 10;
            var oneItem = Builder<OrderLineItem>.CreateListOfSize(1).Build().ToList();
            var twoItems = Builder<OrderLineItem>.CreateListOfSize(2).Build().ToList();
            var build = Builder<Order>.CreateListOfSize(perfcount)
                .All()
                .With(order => order.Items = oneItem)
                .Random(perfcount / 2)
                .With(order => order.Items = twoItems);
            var list = build.Build().ToList();

            var watch = new Stopwatch();
            watch.Start();


            Logger.Trace("Inserting .....");
            watch.Reset(); watch.Start();


            foreach (Order t in list)
            {
                Action<IDbConnection> insert = db => db.InsertAggregateRoot(t);
                insert.ExecuteInEnvironment(Environment);
            }
            

            
            foreach (var order in list)
            {
                order.CustomerId = "NewIdShouldBeStoredInMappingColumn";
            }

            Logger.Trace("Updating .....");
            watch.Reset(); watch.Start();

            //fudge manual update of json table
            Action<IDbConnection> fudge =
                db =>
                    {
                        foreach (var t in list)
                        {
                            var visitor = new MappedPropertyVisitor(AggregateMemberMap.TypeMap);
                            var tableRows = visitor.Visit(t);
                            var newRow = tableRows.OfType<OrderAggregateRootTable>().First();
                            var tableRow = db.GetAggregateRootStorageTable<Order, OrderAggregateRootTable>(t.Id);

                            newRow.RowId = tableRow.RowId;
                            newRow.JsonHash = -1L;
                            DapperExtensions.DapperExtensions.Update(db, newRow);
                        }
                    };
            fudge.ExecuteInEnvironment(Environment);

            Logger.Trace(watch.ElapsedMilliseconds);


            Logger.Trace("Remapping .....");
            watch.Reset(); watch.Start();

            var types = new List<Type>();
            types.Add(typeof(Order));

            Func<IDbConnection, int> remap = db => db.CheckRemapTablesFor(types, perfcount/2);
            var ret = remap.ExecuteInEnvironment(Environment);
            Assert.That(ret, Is.EqualTo(perfcount / 2));
            ret = remap.ExecuteInEnvironment(Environment);
            Assert.That(ret, Is.EqualTo(perfcount / 2));

            Logger.Trace(watch.ElapsedMilliseconds);


            Logger.Trace("Validating .....");
            watch.Reset(); watch.Start();
            Action<IDbConnection> validate =
                db =>
                    {
                        foreach (var res in list
                            .Select(
                                t =>
                                db.GetAggregateRootStorageTable
                                    <Order, OrderAggregateRootTable>(t.Id)))
                        {
                            Assert.That(res.CustomerId,
                                        Is.EqualTo("NewIdShouldBeStoredInMappingColumn"));
                        }
                    };
            validate.ExecuteInEnvironment(Environment);

            validate =
                db =>
                    {
                        var res = db.Query<OrderAggregateRootTable>(
                            "select * from OrderAggregateRootTable where CustomerId <> 'NewIdShouldBeStoredInMappingColumn' ");

                        Assert.That(res.Count(), Is.EqualTo(0));
                    };
            validate.ExecuteInEnvironment(Environment);

            Logger.Trace(watch.ElapsedMilliseconds);
        }
    }
}
