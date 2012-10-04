using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ninject;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Domain;
using Website.Test.Common;

namespace Website.Azure.Common.Tests.TableStorage
{

    class OneEntity : EntityBase<OneEntity>, EntityInterface
    {
        public string Prop { get; set; }
        public string PropTwo { get; set; }
        public string PropThree { get; set; }
        [AggregateMemberEntity]
        public List<TwoEntity> RelatedEntities { get; set; }
        [AggregateMemberEntity]
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
    }

    class ThreeEntity 
    {
        public int SomeProp { get; set; }
        [AggregateMemberEntity]
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
            Kernel.Rebind<TableNameAndPartitionProviderServiceInterface>()
                .To<TableNameAndPartitionProviderService>();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<TableNameAndPartitionProviderServiceInterface>();
        }

        private void register(TableNameAndPartitionProviderServiceInterface tableNameAndPartitionProviderService)
        {

            tableNameAndPartitionProviderService.Add<OneEntity>(0, "testOneEntity", entity => entity.Prop);

            AssertUtil.Count(1, tableNameAndPartitionProviderService.GetAllTableNames());
        }

        [Test]
        public void TableNameAndPartitionProviderServiceRegister()
        {
            register(Kernel.Get<TableNameAndPartitionProviderServiceInterface>());
        }

        [Test]
        public void TableNameAndPartitionProviderServiceReRegisterUpdateEntry()
        {
            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();

            register(tableNameAndPartitionProviderService);


            tableNameAndPartitionProviderService.Add<OneEntity>(0, "testOneEntityNewTable", entity => entity.Prop);
            AssertUtil.Count(1, tableNameAndPartitionProviderService.GetAllTableNames());

            var tableName = tableNameAndPartitionProviderService.GetTableName<OneEntity>(0);
            Assert.AreEqual("testOneEntityNewTable", tableName);
        }

        [Test]
        public void TableNameAndPartitionProviderServiceReturnsMappingForSubClass()
        {
            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();

            register(tableNameAndPartitionProviderService);

           

            var tableName = tableNameAndPartitionProviderService.GetTableName<OneExtend>(0);
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
            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();

            register(tableNameAndPartitionProviderService);

            tableNameAndPartitionProviderService.Add<OneEntity>(1, "OneEntity", entity => entity.PropTwo);
            tableNameAndPartitionProviderService.Add<OneExtend>(1, "OneExtend", entity => entity.PropThree);
            tableNameAndPartitionProviderService.Add<AnotherEnt>(0, "AnotherEnt", entity => entity.Blah);
            tableNameAndPartitionProviderService.Add<OneExtend>(2, "OneExtend2", entity => entity.PropThree);
            tableNameAndPartitionProviderService.Add<AnotherEntExtend>(1, "AnotherEntExtend", entity => entity.BlahTwo);

            var tableName = tableNameAndPartitionProviderService.GetTableName<OneExtend>(0);
            Assert.AreEqual("testOneEntity", tableName);

            tableName = tableNameAndPartitionProviderService.GetTableName<OneExtend>(1);
            Assert.AreEqual("OneExtend", tableName);

            tableName = tableNameAndPartitionProviderService.GetTableName<OneExtendExtend>(1);
            Assert.AreEqual("OneExtend", tableName);

            tableName = tableNameAndPartitionProviderService.GetTableName<AnotherEntExtend>(0);
            Assert.AreEqual("AnotherEnt", tableName);

            tableName = tableNameAndPartitionProviderService.GetTableName<AnotherEntExtend>(1);
            Assert.AreEqual("AnotherEntExtend", tableName);

            tableName = tableNameAndPartitionProviderService.GetTableName<OneExtendExtend>(2);
            Assert.AreEqual("OneExtend2", tableName);

            tableName = tableNameAndPartitionProviderService.GetTableName<OneExtendExtend>(0);
            Assert.AreEqual("testOneEntity", tableName);
        }

        [Test]
        public void TableNameAndPartitionProviderServiceGetPartitionIdentifiers()
        {
            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();

            register(tableNameAndPartitionProviderService);
            tableNameAndPartitionProviderService.Add<OneEntity>(1, "testOneEntity", entity => entity.Prop);

            //add another entity type
            tableNameAndPartitionProviderService.Add<TwoEntity>(0, "testTwoEntity", entity => entity.Prop);

            var partitions =
                tableNameAndPartitionProviderService.GetPartitionIdentifiers<OneEntity>();
            AssertUtil.Count(2, partitions);
            CollectionAssert.Contains(partitions, 0);
            CollectionAssert.Contains(partitions, 1);
        }

        [Test]
        public void TableNameAndPartitionProviderServiceGetGetPartitionKeyFunc()
        {
            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();

            Func<OneEntity, string> partKey = entity => entity.Prop;

            tableNameAndPartitionProviderService.Add<OneEntity>(0, "testOneEntity", partKey);
            tableNameAndPartitionProviderService.Add<OneEntity>(1, "testOneEntity", entity => entity.Prop);

            var tablePartitonFunc = tableNameAndPartitionProviderService.GetPartitionKeyFunc<OneEntity>(0);
            Assert.IsNotNull(tablePartitonFunc);
        }

        [Test]
        public void TableNameAndPartitionProviderServiceGetGetRowKeyFunc()
        {
            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();

            Func<OneEntity, string> partKey = entity => entity.Prop;
            Func<OneEntity, string> rowKey = entity => entity.Prop;

            tableNameAndPartitionProviderService.Add<OneEntity>(0, "testOneEntity", partKey, rowKey);
            tableNameAndPartitionProviderService.Add<OneEntity>(1, "testOneEntity", entity => entity.Prop, entity => entity.Prop);

            var tableRowFunc = tableNameAndPartitionProviderService.GetRowKeyFunc<OneEntity>(0);
            Assert.IsNotNull(tableRowFunc);
        }
    }
}
