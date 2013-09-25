using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using Dark;
using Website.Azure.Common.Sql;
using Website.Azure.Common.Sql.Infrastructure;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Execution;

namespace Website.Azure.Common.Tests.Sql
{
    [ConfigureAggregateStorage]
    public static class OrdersMapping
    {

        public static void UpgradeSchema(ExecutionEnvironment env)
        {
            var assembly = Assembly.GetExecutingAssembly();

            Action<IDbConnection> tables = db => db.UpgradeSchemaFromAssembly(assembly);
            tables.ExecuteInEnvironment(env);
            Configure();
        }

        public static void RecreateSchema(ExecutionEnvironment env)
        {
            var assembly = Assembly.GetExecutingAssembly();

            Action<IDbConnection> tables = db => db.RunSqlRes(assembly, "drop_table");
            tables.ExecuteInEnvironment(env);

            UpgradeSchema(env);

        }

        public static void Configure()
        {
            OrderAggregateRootTable.Configure();
        }

    }
     
                    
    public class OrderAggregateRootTable : AggregateRootTable
    {
        public static void Configure()
        {
            AutoMapperExtensions.CreateMap<Order, OrderAggregateRootTable>();

            AggregateMemberMap.AddMap<Order, Order, OrderAggregateRootTable>(loc
            => loc, (agg, source, target) => source.MapToInstance(target));

            OrderLineItemRootMemberTable.Configure();
        }
//
//  LastUpdated datetime,
//	CustomerId nvarchar(256) NOT NULL,
//	OrderCredit  decimal(18, 2) NULL,


        public DateTime LastUpdated { get; set; }
        public string CustomerId { get; set; }
        public decimal OrderCredit { get; set; }

    }

    public class OrderLineItemRootMemberTable : AggregateRootMemberTable
    {
        public static void Configure()
        {
            AutoMapperExtensions.CreateMap<OrderLineItem, OrderLineItemRootMemberTable>();

            AggregateMemberMap.AddMap<Order, OrderLineItem, OrderLineItemRootMemberTable>(order => order.Items.First(),
                (deal, product, table) => product.MapToInstance(table));
        }

//	ProductId nvarchar(256) NOT NULL,
//	Price decimal(18, 2) NULL,
//	Quantity int,

        public string Product { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }



    ////////////////////////////////////////////////////////////////////////////////////////////////////////
    public interface OrderInterface : AggregateRootInterface, EntityInterface
    {
        DateTime LastUpdated { get; set; }
        string CustomerId { get; set; }
        string CustomerName { get; set; }
        decimal OrderCredit { get; set; }
        List<OrderLineItem> Items { get; set; }
    }

    /// 

    [Serializable]
    public class Order : EntityBase<OrderInterface>, AggregateRootInterface
    {
        public Order()
        {
            Items = new List<OrderLineItem>();
            LastUpdated = DateTime.Now;
        }

        public DateTime LastUpdated { get; set; }

        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal OrderCredit { get; set; }

        public List<OrderLineItem> Items { get; set; }
    }

    [Serializable]
    public class OrderLineItem 
    {
        public string Product { get; set; }
        public decimal Price { get; set; }

        public int Quantity { get; set; }

    }

}
