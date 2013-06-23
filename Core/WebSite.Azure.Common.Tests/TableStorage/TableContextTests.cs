using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;
using Website.Test.Common;

namespace Website.Azure.Common.Tests.TableStorage
{
    [TestFixture("dev")]
    [TestFixture("real")]
    public class TableContextTests
    {
        static StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        public TableContextTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Rebind<TableContextInterface>()
                .To<TableContext>();

            Kernel.Rebind<TableNameAndIndexProviderServiceInterface>()
                .To<TableNameAndIndexProviderService>()
                .InSingletonScope();

            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndIndexProviderServiceInterface>();
            tableNameAndPartitionProviderService.Add<OneEntity>("testOneEntity", entity => entity.Prop);

            var context = Kernel.Get<TableContextInterface>();

            foreach(var tableName in tableNameAndPartitionProviderService.GetAllTableNames())
            {
                context.InitTable<MyExtendableTableEntry>(tableName);
            }

            context.Delete<MyExtendableTableEntry>("testOneEntity", null);
            context.SaveChanges();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<TableNameAndIndexProviderServiceInterface>();
            Kernel.Unbind<TableContextInterface>();
            AzureEnv.UseRealStorage = false;
        }

        public static Dictionary<string, List<TableEntryType>> SetupMockTableContext<TableEntryType>(MoqMockingKernel kernel, Dictionary<string, List<TableEntryType>> mockStore)
        {
            var mockTableContext = new Moq.Mock<TableContextInterface>();
            Action<string> ensureTable = s =>
                                             {if(!mockStore.ContainsKey(s))mockStore.Add(s, new List<TableEntryType>());};

            mockTableContext.Setup(
                tc =>
                tc.PerformQuery(It.IsAny<string>(), It.IsAny<Expression<Func<TableEntryType, bool>>>(),
                                It.IsAny<int>()))
                .Returns<string, Expression<Func<TableEntryType, bool>>, int>(
                    (table, query, take) =>
                        {
                            ensureTable(table);
                            var ret = mockStore[table].AsQueryable();
                            if (query != null)
                                ret = ret.Where(query);
                            if (take >= 0)
                                ret = ret.Take(take);
                            return ret;
                        });

            mockTableContext.Setup(
                    tc =>
                    tc.PerformSelectQuery(It.IsAny<string>(), It.IsAny<Expression<Func<TableEntryType, bool>>>()
                    , It.IsAny<Expression<Func<TableEntryType, StorageTableKey>>>(), It.IsAny<int>()))
                    .Returns<string, Expression<Func<TableEntryType, bool>>
                    , Expression<Func<TableEntryType, StorageTableKey>> 
                    ,int>(
                        (table, query, select, take) =>
                        {
                            ensureTable(table);
                            var ret = mockStore[table].AsQueryable();
                            if (query != null)
                                ret = ret.Where(query);
                            if (take >= 0)
                                ret = ret.Take(take);
                            return ret.Select(select);
                        });

            mockTableContext.Setup(tc => tc.Store(It.IsAny<string>(), It.IsAny<StorageTableKeyInterface>()))
                .Callback<string, StorageTableKeyInterface>((table, entry) =>
                                                                  {
                                                                      ensureTable(table);
                                                                      var store = mockStore[table];
                                                                      if(!store.Contains((TableEntryType) entry))
                                                                          store.Add((TableEntryType)entry);                                                                     

                                                                  });

            kernel.Rebind<TableContextInterface>().ToConstant(mockTableContext.Object);
            return mockStore;
        }


        [Test]
        public void TableContextCanSerializeDynamicProperties()
        {
            var dynamicEntity = new ExtendableTableEntry {RowKey = "123", PartitionKey = "123", PartitionClone = 0, KeyChanged =  false};
            FillEntityWithEdmTypes(dynamicEntity);
            var tabCtx = Kernel.Get<TableContextInterface>();


            Assert.IsNotNull(tabCtx);

            tabCtx.Store("testOneEntity", dynamicEntity);
            tabCtx.SaveChanges();

            var tabCtxQuery = Kernel.Get<TableContextInterface>();
            Assert.IsNotNull(tabCtxQuery);

            var ret = tabCtxQuery.PerformQuery<ExtendableTableEntry>
                ("testOneEntity", query: t => t.PartitionKey == dynamicEntity.PartitionKey && t.RowKey == dynamicEntity.RowKey)
                .SingleOrDefault();

            Assert.IsNotNull(ret);

            var source = ConvertToSerialized(dynamicEntity);
            AssertUtil.AssertAreElementsEqualForKeyValPairsIncludeEnumerableValues(source, ret.GetAllProperties());
        }

        [Test]
        public void TableContextCanAddNewDynamicProperty()
        {
            DeleteEntityWithKey("1234");
            DeleteEntityWithKey("123");
            TableContextCanSerializeDynamicProperties();

            var dynamicEntity = new ExtendableTableEntry { RowKey = "1234", PartitionKey = "1234", KeyChanged = false, PartitionClone = 0};
            dynamicEntity["test2"] = 345.5;

            var tabCtx = Kernel.Get<TableContextInterface>();
            Assert.IsNotNull(tabCtx);

            tabCtx.Store("testOneEntity", dynamicEntity);
            tabCtx.SaveChanges();

            var tabCtxQuery = Kernel.Get<TableContextInterface>();
            Assert.IsNotNull(tabCtxQuery);

            var ret = tabCtxQuery.PerformQuery<ExtendableTableEntry>
                ("testOneEntity", query: t => t.PartitionKey == dynamicEntity.PartitionKey && t.RowKey == dynamicEntity.RowKey)
                .SingleOrDefault();

            Assert.IsNotNull(ret);

            var source = ConvertToSerialized(dynamicEntity);
            AssertUtil.AssertAreElementsEqualForKeyValPairsIncludeEnumerableValues(source, ret.GetAllProperties());


            DeleteEntityWithKey("123");
            DeleteEntityWithKey("1234");
        }

        [Test]
        public void TableContextSettingDynamicPropertiesToNullDoesntChangeTheirValWithSaveOptionsMerge()
        {
            DeleteEntityWithKey("123");
            TableContextCanSerializeDynamicProperties();

            var tabCtx = Kernel.Get<TableContextInterface>();
            Assert.IsNotNull(tabCtx);

            var dynamicEntity = tabCtx.PerformQuery<ExtendableTableEntry>
                ("testOneEntity", query: t => t.PartitionKey == "123" && t.RowKey == "123")
                .SingleOrDefault();
            Assert.IsNotNull(dynamicEntity);//keys from AzureTableContextCanSerializeDynamicProperties()
            dynamicEntity["stringval", Edm.String] = null;
            dynamicEntity["int32val", Edm.Int32] = null;

            tabCtx.Store("testOneEntity", dynamicEntity);
            tabCtx.SaveChanges(SaveChangesOptions.None);

            var tabCtxQuery = Kernel.Get<TableContextInterface>();
            Assert.IsNotNull(tabCtxQuery);

            var ret = tabCtxQuery.PerformQuery<ExtendableTableEntry>
                ("testOneEntity", query: t => t.PartitionKey == dynamicEntity.PartitionKey && t.RowKey == dynamicEntity.RowKey)
                .SingleOrDefault();

            Assert.IsNotNull(ret);
            Assert.IsNotNull(ret["stringval"]);
            Assert.IsNotNull(ret["int32val"]);
            Assert.AreEqual("1234", ret["stringval"]);
            Assert.AreEqual(4800, ret["int32val"]);   
        }

        [Test]
        public void TableContextSettingDynamicPropertiesToNullChangesTheirValWithSaveOptionsReplace()
        {
            DeleteEntityWithKey("123");
            TableContextCanSerializeDynamicProperties();

            var tabCtx = Kernel.Get<TableContextInterface>();
            Assert.IsNotNull(tabCtx);

            var dynamicEntity = tabCtx.PerformQuery<ExtendableTableEntry>
                ("testOneEntity", query: t => t.PartitionKey == "123" && t.RowKey == "123")
                .SingleOrDefault();
            Assert.IsNotNull(dynamicEntity);//keys from AzureTableContextCanSerializeDynamicProperties()
            dynamicEntity["stringval", Edm.String] = null;
            dynamicEntity["int32val", Edm.Int32] = null;

            tabCtx.Store("testOneEntity", dynamicEntity);
            tabCtx.SaveChanges(SaveChangesOptions.ReplaceOnUpdate);

            var tabCtxQuery = Kernel.Get<TableContextInterface>();
            Assert.IsNotNull(tabCtxQuery);

            var ret = tabCtxQuery.PerformQuery<ExtendableTableEntry>
                ("testOneEntity", query: t => t.PartitionKey == dynamicEntity.PartitionKey && t.RowKey == dynamicEntity.RowKey)
                .SingleOrDefault();

            Assert.IsNotNull(ret);
            Assert.IsNull(ret["stringval"]);
            Assert.IsNull(ret["int32val"]);
        }

        private void DeleteEntityWithKey(string key)
        {
            var tabCtx = Kernel.Get<TableContextInterface>();
            Assert.IsNotNull(tabCtx);
            tabCtx.Delete<ExtendableTableEntry>("testOneEntity", t => t.PartitionKey == key && t.RowKey == key);
            tabCtx.SaveChanges();
        }

        private Dictionary<EdmProp, object> ConvertToSerialized(ExtendableTableEntry entry)
        {
            var converted = new Dictionary<EdmProp, object>();
            foreach (var prop in entry.GetAllProperties())
            {
                var convert = Edm.ConvertToEdmValue(prop.Key.EdmTyp, prop.Value);
                if(prop.Key.EdmTyp == Edm.Binary && convert != null)
                    convert = Convert.FromBase64String((string)convert);
                converted.Add(prop.Key, convert);
            }
            return converted;
        }

        private void FillEntityWithEdmTypes(ExtendableTableEntry entry)
        {
            //un supported types converting to supported types in serialization
            //"Edm.String"
            entry["stringval"] = "1234";
            //"Edm.Byte"
            entry["byteval"] = (byte)2;
            //"Edm.SByte"
            entry["sbyteval"] = (sbyte)-2;
            //"Edm.Int16"
            entry["int16val"] = (short)4500;
            //"Edm.Single"
            entry["singleval"] = (float)4600.5;
            //"Edm.Decimal" 
            entry["decimalval"] = (decimal)4700.5;

            //"Edm.Int32"
            entry["int32val"] = (int)4800;
            //"Edm.Int64"
            entry["int64val"] = (long)4900;
            //"Edm.Double"
            entry["doubleval"] = (double) 5000.5;   
            //"Edm.Boolean"
            entry["boolval"] = true;
            //"Edm.DateTime"
            entry["datetimeval"] = new DateTime(2001, 1, 1);
            //"Edm.Binary"
            entry["binval"] = new byte[] { 1, 2, 3 };
            //"Edm.Guid"
            entry["guidval"] = new Guid("871531C8-52DD-4A97-8508-37A84E63DD35");
        }

        class MyExtendableTableEntry : ExtendableTableEntry
        {
            public string PropertyOne
            {
                get { return this["PropertyOne"] as string; }
                set { this["PropertyOne", Edm.String] = value; }
            }

            public string PropertyTwo
            {
                get { return this["PropertyTwo"] as string; }
                set { this["PropertyTwo", Edm.String] = value; }
            }
        }

        [Test]
        public void TableContextPerformSelectQueryReturnsASubsetOfEntity()
        {
//            as at at 1.6 sdk Development storage doesn't fucking support projection queries
//            only wasted half a day on this.....
            var myEntity = new MyExtendableTableEntry
                                    {
                                        RowKey = Guid.NewGuid().ToString(),
                                        PartitionKey = Guid.NewGuid().ToString(),
                                        PropertyOne = "PropertyOne",
                                        PropertyTwo = "PropertyTwo"
                                    };

            var tabCtx =  Kernel.Get<TableContextInterface>();
            Assert.IsNotNull(tabCtx);

            tabCtx.Store("testOneEntity", myEntity);
            tabCtx.SaveChanges();

            var tabCtxQuery = Kernel.Get<TableContextInterface>();
            Assert.IsNotNull(tabCtxQuery);

            var ret = tabCtxQuery.PerformQuery<MyExtendableTableEntry>
                ("testOneEntity", query: t => t.PartitionKey == myEntity.PartitionKey && t.RowKey == myEntity.RowKey)
                .SingleOrDefault();

            Assert.AreEqual(myEntity.RowKey, ret.RowKey);
            Assert.AreEqual(myEntity.PartitionKey, ret.PartitionKey);
            Assert.AreEqual(myEntity.PropertyOne, ret.PropertyOne);
            Assert.AreEqual(myEntity.PropertyTwo, ret.PropertyTwo);

            tabCtxQuery = Kernel.Get<TableContextInterface>();

            Expression<Func<MyExtendableTableEntry, bool>> query =
                t => t.PartitionKey == myEntity.PartitionKey && t.RowKey == myEntity.RowKey;

            var partialRet = tabCtxQuery.PerformSelectQuery
                ("testOneEntity", query, s => new {PropertyOne = s.PropertyOne})
            .SingleOrDefault();

            Assert.IsNotNull(partialRet);
            Assert.AreEqual(myEntity.PropertyOne, partialRet.PropertyOne);
        }

        [Test]
        public void TableContextPerformParallelQueriesReturnsAllResultsJoinedTogether()
        {
            var tabCtx = Kernel.Get<TableContextInterface>();
            Assert.IsNotNull(tabCtx);
            var partitions = new string[]
                                         {
                                             Guid.NewGuid().ToString(), Guid.NewGuid().ToString(),
                                             Guid.NewGuid().ToString(), Guid.NewGuid().ToString(),
                                         };
            int count = 0;
            foreach (var partition in partitions)
            {
                var myEntity = new MyExtendableTableEntry
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = partition,
                    PropertyOne = count.ToString(CultureInfo.InvariantCulture),
                };
                count++;
                tabCtx.Store("testOneEntity", myEntity );
            }
            tabCtx.SaveChanges();

            var searchQueries = new Expression<Func<MyExtendableTableEntry, bool>>[]
                                    {
                                        entry => entry.PartitionKey == partitions[0],
                                        entry => entry.PartitionKey == partitions[1],
                                        entry => entry.PartitionKey == partitions[2],
                                        entry => entry.PartitionKey == partitions[3]
                                    };

            var ret = tabCtx.PerformParallelQueries("testOneEntity", searchQueries);
            AssertUtil.Count(4, ret);
            foreach (var partition in partitions)
            {
                AssertUtil.Count(1, ret.Where(t => t.PartitionKey == partition));
            }
            
        }

        [Test]
        public void TableContextPerformParallelSelectQueriesReturnsAllResultsJoinedTogether()
        {
            var tabCtx = Kernel.Get<TableContextInterface>();
            Assert.IsNotNull(tabCtx);
            var partitions = new string[]
                                         {
                                             Guid.NewGuid().ToString(), Guid.NewGuid().ToString(),
                                             Guid.NewGuid().ToString(), Guid.NewGuid().ToString(),
                                         };
            int count = 0;
            foreach (var partition in partitions)
            {
                var myEntity = new MyExtendableTableEntry
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = partition,
                    PropertyOne = count.ToString(CultureInfo.InvariantCulture),
                };
                count++;
                tabCtx.Store("testOneEntity", myEntity);
            }
            tabCtx.SaveChanges();

            var searchQueries = new Expression<Func<MyExtendableTableEntry, bool>>[]
                                    {
                                        entry => entry.PartitionKey == partitions[0],
                                        entry => entry.PartitionKey == partitions[1],
                                        entry => entry.PartitionKey == partitions[2],
                                        entry => entry.PartitionKey == partitions[3]
                                    };

            var ret = tabCtx.PerformParallelSelectQueries("testOneEntity", searchQueries, s => new { PropertyOne = s.PropertyOne });
            AssertUtil.Count(4, ret);
            for (int i = 0; i < ret.Count(); i++ )
            {
                AssertUtil.Count(1, ret.Where(t => t.PropertyOne == i.ToString(CultureInfo.InvariantCulture)));
            }

        }
    }
}
