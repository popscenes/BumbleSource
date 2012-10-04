﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ninject;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Domain;

namespace Website.Azure.Common.Tests.TableStorage
{

    class JsonTestEntity
    {
        public string Prop { get; set; }
        public string PropTwo { get; set; }
        public string PropThree { get; set; }
        public HashSet<string> HashTest { get; set; }
        [AggregateMemberEntity]
        public List<TwoEntity> SubEntity { get; set; }
    }



    [TestFixture("dev")]
    [TestFixture("real")]
    public class JsonTableEntryTests
    {
        static StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        public JsonTableEntryTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Rebind<TableContextInterface>()
                .To<TableContext>();

            Kernel.Rebind<TableNameAndPartitionProviderServiceInterface>()
                .To<TableNameAndPartitionProviderService>()
                .InSingletonScope();

            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();
            tableNameAndPartitionProviderService.Add<JsonTestEntity>(0, "testJsonEntity", entity => entity.Prop, entity => entity.PropTwo);

            var context = Kernel.Get<TableContextInterface>();

            foreach (var tableName in tableNameAndPartitionProviderService.GetAllTableNames())
            {
                context.InitTable<JsonTableEntry>(tableName);
            }

            context.Delete<JsonTableEntry>("testJsonEntity", null, 0);
            context.SaveChanges();
        }


        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<TableNameAndPartitionProviderServiceInterface>();
            Kernel.Unbind<TableContextInterface>();
            AzureEnv.UseRealStorage = false;
        }

        internal static void AssertAreEqual(JsonTestEntity one, JsonTestEntity two)
        {
            Assert.AreEqual(one.Prop, two.Prop);
            Assert.AreEqual(one.PropTwo, two.PropTwo);
            Assert.AreEqual(one.PropThree, two.PropThree);

            for (var i = 0; i < one.SubEntity.Count; i++)
            {
                TwoEntity.AssertAreEqual(one.SubEntity[i], two.SubEntity[i]);
            }

            CollectionAssert.AreEqual(one.HashTest, two.HashTest);
        }

        [Test]
        public void JsonTableEntryTestStoreRetrieve()
        {
            var testob = new JsonTestEntity()
                             {
                                 Prop = "Hello",
                                 PropTwo = "There",
                                 PropThree = "Yo",
                                 HashTest = new HashSet<string>(){"one", "two"},
                                 SubEntity = new List<TwoEntity>(){new TwoEntity(){Prop = "TwoProp",PropTwo = null}}
                             };

            var nameAndPartitionProviderService = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();
            var partKeyFunc = nameAndPartitionProviderService.GetPartitionKeyFunc<JsonTestEntity>(0);
            var rowKeyFunc = nameAndPartitionProviderService.GetRowKeyFunc<JsonTestEntity>(0);


            var tableEntry = new JsonTableEntry()
                                 {
                                     KeyChanged = false,
                                     PartitionKey = partKeyFunc(testob),
                                     RowKey = rowKeyFunc(testob)
                                 };
            tableEntry.UpdateEntry(testob);

            var tableName = nameAndPartitionProviderService.GetTableName<JsonTestEntity>(0);
            var tabCtx = Kernel.Get<TableContextInterface>();
            tabCtx.Store(tableName, tableEntry);
            tabCtx.SaveChanges();

            var tabCtxRet = Kernel.Get<TableContextInterface>();
            var ret = tabCtxRet.PerformQuery<JsonTableEntry>(tableName, e => e.PartitionKey == tableEntry.PartitionKey 
                                                                   && e.RowKey == tableEntry.RowKey)
                                                                   .SingleOrDefault();

            Assert.IsNotNull(ret);
            var deserialized = ret.GetEntity<JsonTestEntity>();
            AssertAreEqual(testob, deserialized);
        }

        [Test]
        public void JsonTableEntryTestStoreLargeEntity()
        {
//            if (AzureEnv.UseRealStorage)//will take too long to run
//                return;

            int numChars = 1024 * 128;//256k utf16
            var strBld = new StringBuilder(numChars);
            strBld.Append('a', numChars);

            var testob = new JsonTestEntity()
            {
                Prop = "Hello1",
                PropTwo = "There2",
                PropThree = strBld.ToString(),
                HashTest = new HashSet<string>() { "one", "two" },
                SubEntity = new List<TwoEntity>() { new TwoEntity() { Prop = "TwoProp", PropTwo = null } }
            };



            var nameAndPartitionProviderService = Kernel.Get<TableNameAndPartitionProviderServiceInterface>();
            var partKeyFunc = nameAndPartitionProviderService.GetPartitionKeyFunc<JsonTestEntity>(0);
            var rowKeyFunc = nameAndPartitionProviderService.GetRowKeyFunc<JsonTestEntity>(0);


            var tableEntry = new JsonTableEntry()
            {
                KeyChanged = false,
                PartitionKey = partKeyFunc(testob),
                RowKey = rowKeyFunc(testob)
            };
            tableEntry.UpdateEntry(testob);

            var tableName = nameAndPartitionProviderService.GetTableName<JsonTestEntity>(0);
            var tabCtx = Kernel.Get<TableContextInterface>();
            tabCtx.Store(tableName, tableEntry);
            tabCtx.SaveChanges();

            var tabCtxRet = Kernel.Get<TableContextInterface>();
            var ret = tabCtxRet.PerformQuery<JsonTableEntry>(tableName, e => e.PartitionKey == tableEntry.PartitionKey
                                                                   && e.RowKey == tableEntry.RowKey)
                                                                   .SingleOrDefault();

            Assert.IsNotNull(ret);
            var deserialized = ret.GetEntity<JsonTestEntity>();
            AssertAreEqual(testob, deserialized);
        }
    }
}
