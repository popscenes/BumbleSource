using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using NUnit.Framework;
using Website.Azure.Common.Sql;
using Website.Azure.Common.Sql.Infrastructure;

namespace Website.Azure.Common.Tests.Sql
{
    //note localdb is slow for tests just east to use, you can change connection string
    [TestFixture]
    public class PerfTests : ArmTestBase
    {
        //private const int Perfcount = 10000;
        private const int Perfcount = 10;

        [Test]
        public void Insert_IntoTable_FromAggregate_Map_Structure_Performance()
        {

            var oneItem = Builder<OrderLineItem>.CreateListOfSize(1).Build().ToList();
            var twoItems = Builder<OrderLineItem>.CreateListOfSize(2).Build().ToList();
            var build = Builder<Order>.CreateListOfSize(Perfcount)
                .All()
                .With(order => order.Id = null)
                .With(order => order.Items = oneItem)
                .Random(Perfcount / 2)
                .With(order => order.Items = twoItems);
            var list = build.Build().ToList();

            var watch = new Stopwatch();
            watch.Start();


            for (int i = 0; i < list.Count; i++)
            {
                var ord = list[i];
                Action<IDbConnection> insert = db => db.InsertAggregateRoot(ord);
                insert.ExecuteInEnvironment(Environment);
                if (i % 1000 == 0)
                    Logger.Trace(i + " ms " + watch.ElapsedMilliseconds);
            }
            


            Logger.Trace(watch.ElapsedMilliseconds);

            Logger.Trace("Validating .....");
            watch.Reset(); watch.Start();

            for (int i = 0; i < Perfcount; i++)
            {
                var id = list[i].Id;
                Func<IDbConnection, Order> query = db => db.GetAggregateRoot<Order>(id);

                var res = query.ExecuteInEnvironment(Environment);
                if (i % 1000 == 0)
                    Logger.Trace(i + " ms " + watch.ElapsedMilliseconds);
                Assert.That(res.CustomerId, Is.EqualTo(list[i].CustomerId));
            }
            
            Logger.Trace(watch.ElapsedMilliseconds);

            //Assert.That(records.Count, Is.GreaterThan(0));
        }

        [Test]
        public void UpdateTable_FromAggregate_Map_Structure_Performance()
        {

            var oneItem = Builder<OrderLineItem>.CreateListOfSize(1).Build().ToList();
            var twoItems = Builder<OrderLineItem>.CreateListOfSize(2).Build().ToList();
            var build = Builder<Order>.CreateListOfSize(Perfcount)
                .All()
                .With(order => order.Id = null)
                .With(order => order.Items = oneItem)
                .Random(Perfcount / 2)
                .With(order => order.Items = twoItems);
            var list = build.Build().ToList();


            var watch = new Stopwatch();
            watch.Start();
            var Inc = 0L;

            list.ForEach((order) =>
                {
                    Action<IDbConnection> insert = db => db.InsertAggregateRoot(order);
                    insert.ExecuteInEnvironment(Environment);

                    var num = Interlocked.Increment(ref Inc);
                    if (num % 1000 == 0)
                        Logger.Trace(Inc + " ms " + watch.ElapsedMilliseconds);

                });

            Logger.Trace(Perfcount + " " + watch.ElapsedMilliseconds);

            Logger.Trace("Updating .....");
            watch.Reset(); watch.Start();

            Inc = 0;

            list.ForEach((order) =>
                {
                    Action<IDbConnection> update = db => db.UpdateAggregateRoot<Order>(order.Id
                        , agg =>
                        {
                            agg.Items.First().Quantity = 445;
                            agg.CustomerId = "234566";
                        });
                    update.ExecuteInEnvironment(Environment);

                    var num = Interlocked.Increment(ref Inc);
                    if (num % 1000 == 0)
                        Logger.Trace(Inc + " ms " + watch.ElapsedMilliseconds);
                });


            Logger.Trace(Perfcount + " " + watch.ElapsedMilliseconds);

            Logger.Trace("Validating .....");
            watch.Reset(); watch.Start();


            Inc = 0;
            list.ForEach((order) =>
            {
                Func<IDbConnection, Order> query = db => db.GetAggregateRoot<Order>(order.Id);
                var res = query.ExecuteInEnvironment(Environment);
                Assert.That(res.CustomerId, Is.EqualTo("234566"));

                var num = Interlocked.Increment(ref Inc);
                if (num%1000 == 0)
                    Logger.Trace(Inc + " ms " + watch.ElapsedMilliseconds);
            });

            Logger.Trace(Perfcount + " " + watch.ElapsedMilliseconds);
        }

        [Test]
        public void UpdateTable_FromAggregate_Map_Structure_Performance_Parallel()
        {

            var oneItem = Builder<OrderLineItem>.CreateListOfSize(1).Build().ToList();
            var twoItems = Builder<OrderLineItem>.CreateListOfSize(2).Build().ToList();
            var build = Builder<Order>.CreateListOfSize(Perfcount)
                .All()
                .With(order => order.Id = null)
                .With(order => order.Items = oneItem)
                .Random(Perfcount / 2)
                .With(order => order.Items = twoItems);
            var list = build.Build().ToList();


            var watch = new Stopwatch();
            watch.Start();
            var Inc = 0L;


            Parallel.ForEach(list
                , (order, state) =>
                {

                    Action<IDbConnection> insert = db => db.InsertAggregateRoot(order);
                    insert.ExecuteInEnvironment(Environment);

                    var num = Interlocked.Increment(ref Inc);
                    if (num % 1000 == 0)
                        Logger.Trace(Inc + " ms " + watch.ElapsedMilliseconds);

                });

            Logger.Trace(Perfcount + " " + watch.ElapsedMilliseconds);

            Logger.Trace("Updating .....");
            watch.Reset(); watch.Start();

            Inc = 0;

            Parallel.ForEach(list
                , (order, state) =>
                {
                    Action<IDbConnection> update = db => db.UpdateAggregateRoot<Order>(order.Id
                        , agg =>
                        {
                            agg.Items.First().Quantity = 445;
                            agg.CustomerId = "234566";
                        });
                    update.ExecuteInEnvironment(Environment);

                    var num = Interlocked.Increment(ref Inc);
                    if (num % 1000 == 0)
                        Logger.Trace(Inc + " ms " + watch.ElapsedMilliseconds);
                });


            Logger.Trace(Perfcount + " " + watch.ElapsedMilliseconds);

            Logger.Trace("Validating .....");
            watch.Reset(); watch.Start();


            Inc = 0;
            Parallel.ForEach(list
                , (order, state) =>
                {
                    Func<IDbConnection, Order> query = db => db.GetAggregateRoot<Order>(order.Id);
                    var res = query.ExecuteInEnvironment(Environment);
                    Assert.That(res.CustomerId, Is.EqualTo("234566"));

                    var num = Interlocked.Increment(ref Inc);
                    if (num % 1000 == 0)
                        Logger.Trace(Inc + " ms " + watch.ElapsedMilliseconds);
                });

            Logger.Trace(Perfcount + " " + watch.ElapsedMilliseconds);

        }
    }
}
