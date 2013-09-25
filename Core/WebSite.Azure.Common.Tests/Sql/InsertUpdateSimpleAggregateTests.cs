using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Website.Azure.Common.Sql;
using Website.Azure.Common.Sql.Infrastructure;

namespace Website.Azure.Common.Tests.Sql
{


    [TestFixture]
    public class InsertUpdateSimpleAggregateTests : ArmTestBase
    {


        [Test]
        public void Create_Aggregate_TableStructure()
        {

            var newOrder = new Order()
                            {
                                Id = "123",
                                CustomerId = "234565",
                                Items = new List<OrderLineItem>() {new OrderLineItem() {Quantity = 444}}
                            };

            var vis = new MappedPropertyVisitor(AggregateMemberMap.Map);
            var records = vis.Visit(newOrder);


            Assert.That(records.Count, Is.EqualTo(2));
        }



        [Test]
        public void Insert_IntoTable_FromAggregate_Map_Structure()
        {

            var newOrder = new Order()
            {
                CustomerId = "234565",
                Items = new List<OrderLineItem>() { new OrderLineItem() { Quantity = 444 } }
                
            };

            Action<IDbConnection> insert = db => db.InsertAggregateRoot(newOrder);
            insert.ExecuteInEnvironment(Environment);


            Func<IDbConnection, Order> query = db => db.GetAggregateRoot<Order>(newOrder.Id);
            var res = query.ExecuteInEnvironment(Environment);

            Assert.That(res.CustomerId, Is.EqualTo("234565"));
            Assert.That(res.Id, Is.EqualTo(newOrder.Id));
        }




        [Test]
        public void UpdateTable_FromAggregate_Map_Structure()
        {


            var newOrder = new Order()
            {
                CustomerId = "234565",
                Items = new List<OrderLineItem>() { new OrderLineItem() { Quantity = 444 } }
            };


            Action<IDbConnection> insert = db => db.InsertAggregateRoot(newOrder);
            insert.ExecuteInEnvironment(Environment);


            newOrder.Items.First().Quantity = 445;
            newOrder.CustomerId = "234566";


            Action<IDbConnection> update = db 
                => db.UpdateAggregateRoot<Order>(newOrder.Id
                    , order =>
                            {
                                order.Items.First().Quantity =
                                    445;
                                order.CustomerId = "234566";
                            });
            update.ExecuteInEnvironment(Environment);

            Func<IDbConnection, Order> query = db => db.GetAggregateRoot<Order>(newOrder.Id);
            var res = query.ExecuteInEnvironment(Environment);

            Assert.That(res.CustomerId, Is.EqualTo("234566"));

            //Assert.That(records.Count, Is.GreaterThan(0));
        }




    }
}


