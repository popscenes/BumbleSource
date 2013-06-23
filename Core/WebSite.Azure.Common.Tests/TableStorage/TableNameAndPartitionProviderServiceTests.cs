using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ninject;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Domain;
using Website.Test.Common;

namespace Website.Azure.Common.Tests.TableStorage
{

    class OneEntity : EntityBase<OneEntity>, AggregateRootInterface, EntityInterface
    {
        public string Prop { get; set; }
        public string PropTwo { get; set; }
        public string PropThree { get; set; }
        public List<TwoEntity> RelatedEntities { get; set; }
        public ThreeEntity MemberEntity { get; set; }

        internal static void AssertAreEqual(OneEntity one, OneEntity two)
        {
            Assert.AreEqual(one.Id, two.Id);
            Assert.AreEqual(one.Prop, two.Prop);
            Assert.AreEqual(one.PropTwo, two.PropTwo);
            Assert.AreEqual(one.PropThree, two.PropThree);

            for (var i = 0; i < one.RelatedEntities.Count; i++ )
            {
                TwoEntity.AssertAreEqual(one.RelatedEntities[i], two.RelatedEntities[i]);
            }
            
            ThreeEntity.AssertAreEqual(one.MemberEntity, two.MemberEntity);
        }
    }

    class TwoEntity : EntityBase<TwoEntity>, EntityInterface, AggregateInterface
    {
        public TwoEntity()
        {
            Id = Guid.NewGuid().ToString();
        }
        public string Prop { get; set; }
        public string PropTwo { get; set; }

        internal static void AssertAreEqual(TwoEntity one, TwoEntity two)
        {
            Assert.AreEqual(one.Id, two.Id);
            Assert.AreEqual(one.Prop, two.Prop);
            Assert.AreEqual(one.PropTwo, two.PropTwo);
        }

        public string AggregateId { get; set; }
        public string AggregateTypeTag { get; set; }
    }

    class ThreeEntity 
    {
        public int SomeProp { get; set; }
        public TwoEntity MemberEntity { get; set; }

        internal static void AssertAreEqual(ThreeEntity one, ThreeEntity two)
        {
            Assert.AreEqual(one.SomeProp, two.SomeProp);
            TwoEntity.AssertAreEqual(one.MemberEntity, two.MemberEntity);
        }

    }

    class OneExtend : OneEntity
    {
        
    }

    class OneExtendExtend : OneExtend
    {

    }

    [TestFixture]
    public class TableNameAndPartitionProviderServiceTests
    {
        static StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Rebind<TableNameAndIndexProviderServiceInterface>()
                .To<TableNameAndIndexProviderService>();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<TableNameAndIndexProviderServiceInterface>();
        }

        private void register(TableNameAndIndexProviderServiceInterface tableNameAndIndexProviderService)
        {

            tableNameAndIndexProviderService.Add<OneEntity>("testOneEntity", entity => entity.Prop);

            AssertUtil.Count(1, tableNameAndIndexProviderService.GetAllTableNames());
        }

        [Test]
        public void TableNameAndPartitionProviderServiceRegister()
        {
            register(Kernel.Get<TableNameAndIndexProviderServiceInterface>());
        }

        [Test]
        public void TableNameAndPartitionProviderServiceReRegisterUpdateEntry()
        {
            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndIndexProviderServiceInterface>();

            register(tableNameAndPartitionProviderService);


            tableNameAndPartitionProviderService.Add<OneEntity>("testOneEntityNewTable", entity => entity.Prop);
            AssertUtil.Count(1, tableNameAndPartitionProviderService.GetAllTableNames());

            var tableName = tableNameAndPartitionProviderService.GetTableName<OneEntity>();
            Assert.AreEqual("testOneEntityNewTable", tableName);
        }

        [Test]
        public void TableNameAndPartitionProviderServiceReturnsMappingForSubClass()
        {
            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndIndexProviderServiceInterface>();

            register(tableNameAndPartitionProviderService);

           

            var tableName = tableNameAndPartitionProviderService.GetTableName<OneExtend>();
            Assert.AreEqual("testOneEntity", tableName);
        }

        class AnotherEnt
        {
            public string Blah { get; set; }
        }
        class AnotherEntExtend : AnotherEnt 
        {
            public string BlahTwo { get; set; }
        }
        [Test]
        public void TableNameAndPartitionProviderServiceReturnsMappingForClosestSubClass()
        {
            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndIndexProviderServiceInterface>();

            register(tableNameAndPartitionProviderService);

            
            tableNameAndPartitionProviderService.Add<AnotherEnt>("AnotherEnt", entity => entity.Blah);
            

            var tableName = tableNameAndPartitionProviderService.GetTableName<OneExtend>();
            Assert.AreEqual("testOneEntity", tableName);

            tableNameAndPartitionProviderService.Add<OneExtend>("OneExtend", entity => entity.PropThree);

            tableName = tableNameAndPartitionProviderService.GetTableName<OneExtend>();
            Assert.AreEqual("OneExtend", tableName);

            tableName = tableNameAndPartitionProviderService.GetTableName<OneExtendExtend>();
            Assert.AreEqual("OneExtend", tableName);

            tableName = tableNameAndPartitionProviderService.GetTableName<AnotherEntExtend>();
            Assert.AreEqual("AnotherEnt", tableName);

            tableNameAndPartitionProviderService.Add<AnotherEntExtend>("AnotherEntExtend", entity => entity.BlahTwo);
            tableName = tableNameAndPartitionProviderService.GetTableName<AnotherEntExtend>();
            Assert.AreEqual("AnotherEntExtend", tableName);

            tableName = tableNameAndPartitionProviderService.GetTableName<OneExtendExtend>();
            Assert.AreEqual("OneExtend", tableName);
        }

//        [Test]
//        public void TableNameAndPartitionProviderServiceGetPartitionIdentifiers()
//        {
//            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();
//
//            register(tableNameAndPartitionProviderService);
//            tableNameAndPartitionProviderService.Add<OneEntity>(1, "testOneEntity", entity => entity.Prop);
//
//            //add another entity type
//            tableNameAndPartitionProviderService.Add<TwoEntity>(0, "testTwoEntity", entity => entity.Prop);
//
//            var partitions =
//                tableNameAndPartitionProviderService.GetPartitionIdentifiers<OneEntity>();
//            AssertUtil.Count(2, partitions);
//            CollectionAssert.Contains(partitions, 0);
//            CollectionAssert.Contains(partitions, 1);
//        }

        [Test]
        public void TableNameAndPartitionProviderServiceGetGetPartitionKeyFunc()
        {
            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndIndexProviderServiceInterface>();

            Func<OneEntity, string> partKey = entity => entity.Prop;

            tableNameAndPartitionProviderService.Add<OneEntity>("testOneEntity", partKey);

            var tablePartitonFunc = tableNameAndPartitionProviderService.GetPartitionKeyFunc<OneEntity>();
            Assert.IsNotNull(tablePartitonFunc);
        }

        [Test]
        public void TableNameAndPartitionProviderServiceGetGetRowKeyFunc()
        {
            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndIndexProviderServiceInterface>();

            Func<OneEntity, string> partKey = entity => entity.Prop;
            Func<OneEntity, string> rowKey = entity => entity.Prop;

            tableNameAndPartitionProviderService.Add<OneEntity>("testOneEntity", partKey, rowKey);

            var tableRowFunc = tableNameAndPartitionProviderService.GetRowKeyFunc<OneEntity>();
            Assert.IsNotNull(tableRowFunc);
        }
    }
}
