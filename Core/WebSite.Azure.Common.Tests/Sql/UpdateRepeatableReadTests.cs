using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using FizzWare.NBuilder;
using NUnit.Framework;
using Website.Azure.Common.Sql;
using Website.Azure.Common.Sql.Infrastructure;

namespace Website.Azure.Common.Tests.Sql
{
    [TestFixture]
    public class UpdateRepeatableReadTests : ArmTestBase
    {

        [Test]
        public void ReadsAreNotLockedDuringUpdates()
        {
            var twoItems = Builder<OrderLineItem>.CreateListOfSize(2).Build().ToList();
            var build = Builder<Order>.CreateNew()
                .With(order => order.Items = twoItems);
            var newOrder = build.Build();

            Action<IDbConnection> insert = db => db.InsertAggregateRoot(newOrder);
            insert.ExecuteInEnvironment(Environment);

            var longUpdateAndReads
                = new List<Action>()
                      {
                          () =>
                              {
                                  
                                Action<IDbConnection> update = 
                                    db => db.UpdateAggregateRoot<Order>(newOrder.Id, order =>
                                    {
                                        Thread.Sleep(400);
                                        order.CustomerId = "77777";
                                    } );
                                update.ExecuteInEnvironment(Environment);
                              },
                            () =>
                                {
                                    Thread.Sleep(50);//give a chance for the above update to start first
                                    Order res = null;

                                        var cnt = 0;
                                        do
                                        {
                                            Func<IDbConnection, Order> query = 
                                                db => 
                                                    res = db.GetAggregateRoot<Order>(newOrder.Id);

                                            res = query.ExecuteInEnvironment(Environment);
                                            //test we can get the mapping tables as wll

                                            Func<IDbConnection, List<OrderLineItemRootMemberTable>> querylines =
                                                db =>
                                                db.Query<OrderLineItemRootMemberTable>(
                                                    "select * from OrderLineItemRootMemberTable where Id=@Id",
                                                    new {Id = newOrder.Id}).ToList();

                                            var lines = querylines.ExecuteInEnvironment(Environment);
                                            Assert.That(lines.Count, Is.GreaterThan(0));
                                            cnt++;
                                        } while (res.CustomerId != "77777");
                                        Assert.That(cnt > 1);
                                        Logger.Trace("Read {0} times during update", cnt);
                                    

                                }

                      };
            Parallel.ForEach(longUpdateAndReads, action =>
                                                 action());

        }
        [Test]
        public void UpdatesAreConsistentWithRepeatableRead()
        {
            var twoItems = Builder<OrderLineItem>.CreateListOfSize(2).Build().ToList();
            var build = Builder<Order>.CreateNew()
                .With(order => order.Items = twoItems);
            var newOrder = build.Build();

            Action<IDbConnection> insert = db => db.InsertAggregateRoot(newOrder);
            insert.ExecuteInEnvironment(Environment);


            var differentMutations 
                = new List<Action<Order>>()
                      {
                          order =>
                            {
                                order.Items.First().Quantity = 445;
                            },
                          order =>
                            {
                                order.CustomerName = "NewName";
                            },
                          order =>
                            {
                                order.Items.Skip(1).First().Price = 33333;
                            },
                          order =>
                            {
                                order.CustomerId = "234566";
                            },
                        order =>
                            {
                                order.LastUpdated = new DateTime(2030, 11, 11);
                            },
                      };

            Parallel.ForEach(differentMutations, 
                action =>
                    {
                        Action<IDbConnection> update = 
                                    db => 
                                        db.UpdateAggregateRoot<Order>(newOrder.Id, action);
                        update.ExecuteInEnvironment(Environment);
                    });


            Func<IDbConnection, Order> query = db 
                => db.GetAggregateRoot<Order>(newOrder.Id);

            var res = query.ExecuteInEnvironment(Environment);

            Assert.That(res.Items.First().Quantity, Is.EqualTo(445));
            Assert.That(res.CustomerName, Is.EqualTo("NewName"));
            Assert.That(res.Items.Skip(1).First().Price, Is.EqualTo(33333));
            Assert.That(res.CustomerId, Is.EqualTo("234566"));
            Assert.That(res.LastUpdated, Is.EqualTo(new DateTime(2030, 11, 11)));
        }
    }
}
