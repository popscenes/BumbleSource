using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using MbUnit.Framework;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;
using WebSite.Azure.Common.Environment;
using WebSite.Azure.Common.TableStorage;
using WebSite.Infrastructure.Domain;
using WebSite.Test.Common;

namespace WebSite.Azure.Common.Tests.TableStorage
{
    [TestFixture]
    public class AzureTableContextTests
    {
        static StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [Row("dev")] 
        [Row("real")]
        public AzureTableContextTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        }

        static AzureTableContextTests()
        {
            Kernel.Bind<AzureTableContext>().ToSelf().Named("azureTableCtxTest");

            Kernel.Bind<TableNameAndPartitionProviderInterface>()
                .ToConstant(new TableNameAndPartitionProvider<EntityInterface>()
                            {
                                {typeof(ExtendableTableEntry), 0, "azureTableCtxTest", f => f.Id, f => f.Id}                       
                            })
                .WhenAnyAnchestorNamed("azureTableCtxTest");

            Kernel.Bind<AzureTableContext>().ToSelf().Named("MyExtendableTableServiceEntityCtxTest");

            Kernel.Bind<TableNameAndPartitionProviderInterface>()
                .ToConstant(new TableNameAndPartitionProvider<EntityInterface>()
                            {
                                {typeof(MyExtendableTableEntry), 0, "MyExtendableTableServiceEntityCtxTest", f => f.Id, f => f.Id}                       
                            })
                .WhenAnyAnchestorNamed("MyExtendableTableServiceEntityCtxTest");
        }

        [FixtureSetUp]
        public void FixtureSetUp()
        {
            var context = Kernel.Get<AzureTableContext>("azureTableCtxTest");
            context.InitFirstTimeUse();
            context.Delete<ExtendableTableEntry>(null, 0);

            context = Kernel.Get<AzureTableContext>("MyExtendableTableServiceEntityCtxTest");
            context.InitFirstTimeUse();
            context.Delete<MyExtendableTableEntry>(null, 0);
            context.SaveChanges();
        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {
            AzureEnv.UseRealStorage = false;
        }


        [Test]
        public void AzureTableContextCanSerializeDynamicProperties()
        {
            var dynamicEntity = new ExtendableTableEntry {RowKey = "123", PartitionKey = "123", PartitionClone = 0, KeyChanged =  false};
            FillEntityWithEdmTypes(dynamicEntity);
            var tabCtx = Kernel.Get<AzureTableContext>("azureTableCtxTest");
            Assert.IsNotNull(tabCtx);

            tabCtx.Store(new List<StorageTableEntryInterface>() { dynamicEntity });
            tabCtx.SaveChanges();

            var tabCtxQuery = Kernel.Get<AzureTableContext>("azureTableCtxTest");
            Assert.IsNotNull(tabCtxQuery);

            var ret = tabCtxQuery.PerformQuery<ExtendableTableEntry>
                (t => t.PartitionKey == dynamicEntity.PartitionKey && t.RowKey == dynamicEntity.RowKey)
                .SingleOrDefault();

            Assert.IsNotNull(ret);

            var source = ConvertToSerialized(dynamicEntity);
            AssertUtil.AssertAreElementsEqualForKeyValPairsIncludeEnumerableValues(source, ret.GetAllProperties());
        }

        [Test]
        public void AzureTableContextCanAddNewDynamicProperty()
        {
            DeleteEntityWithKey("1234");
            DeleteEntityWithKey("123");
            AzureTableContextCanSerializeDynamicProperties();

            var dynamicEntity = new ExtendableTableEntry { RowKey = "1234", PartitionKey = "1234", KeyChanged = false, PartitionClone = 0};
            dynamicEntity["test2"] = 345.5;

            var tabCtx = Kernel.Get<AzureTableContext>("azureTableCtxTest");
            Assert.IsNotNull(tabCtx);

            tabCtx.Store(new List<StorageTableEntryInterface>() { dynamicEntity });
            tabCtx.SaveChanges();

            var tabCtxQuery = Kernel.Get<AzureTableContext>("azureTableCtxTest");
            Assert.IsNotNull(tabCtxQuery);

            var ret = tabCtxQuery.PerformQuery<ExtendableTableEntry>
                (t => t.PartitionKey == dynamicEntity.PartitionKey && t.RowKey == dynamicEntity.RowKey)
                .SingleOrDefault();

            Assert.IsNotNull(ret);

            var source = ConvertToSerialized(dynamicEntity);
            AssertUtil.AssertAreElementsEqualForKeyValPairsIncludeEnumerableValues(source, ret.GetAllProperties());


            DeleteEntityWithKey("123");
            DeleteEntityWithKey("1234");
        }

        [Test]
        public void AzureTableContextSettingDynamicPropertiesToNullDoesntChangeTheirValWithSaveOptionsMerge()
        {
            DeleteEntityWithKey("123");
            AzureTableContextCanSerializeDynamicProperties();

            var tabCtx = Kernel.Get<AzureTableContext>("azureTableCtxTest");
            Assert.IsNotNull(tabCtx);

            var dynamicEntity = tabCtx.PerformQuery<ExtendableTableEntry>
                (t => t.PartitionKey == "123" && t.RowKey == "123")
                .SingleOrDefault();
            Assert.IsNotNull(dynamicEntity);//keys from AzureTableContextCanSerializeDynamicProperties()
            dynamicEntity["stringval", Edm.String] = null;
            dynamicEntity["int32val", Edm.Int32] = null;

            tabCtx.Store(new List<StorageTableEntryInterface>() { dynamicEntity });
            tabCtx.SaveChanges(SaveChangesOptions.None);

            var tabCtxQuery = Kernel.Get<AzureTableContext>("azureTableCtxTest");
            Assert.IsNotNull(tabCtxQuery);

            var ret = tabCtxQuery.PerformQuery<ExtendableTableEntry>
                (t => t.PartitionKey == dynamicEntity.PartitionKey && t.RowKey == dynamicEntity.RowKey)
                .SingleOrDefault();

            Assert.IsNotNull(ret);
            Assert.IsNotNull(ret["stringval"]);
            Assert.IsNotNull(ret["int32val"]);
            Assert.AreEqual("1234", ret["stringval"]);
            Assert.AreEqual(4800, ret["int32val"]);   
        }

        [Test]
        public void AzureTableContextSettingDynamicPropertiesToNullChangesTheirValWithSaveOptionsReplace()
        {
            DeleteEntityWithKey("123");
            AzureTableContextCanSerializeDynamicProperties();

            var tabCtx = Kernel.Get<AzureTableContext>("azureTableCtxTest");
            Assert.IsNotNull(tabCtx);

            var dynamicEntity = tabCtx.PerformQuery<ExtendableTableEntry>
                (t => t.PartitionKey == "123" && t.RowKey == "123")
                .SingleOrDefault();
            Assert.IsNotNull(dynamicEntity);//keys from AzureTableContextCanSerializeDynamicProperties()
            dynamicEntity["stringval", Edm.String] = null;
            dynamicEntity["int32val", Edm.Int32] = null;

            tabCtx.Store(new List<StorageTableEntryInterface>() { dynamicEntity });
            tabCtx.SaveChanges(SaveChangesOptions.ReplaceOnUpdate);

            var tabCtxQuery = Kernel.Get<AzureTableContext>("azureTableCtxTest");
            Assert.IsNotNull(tabCtxQuery);

            var ret = tabCtxQuery.PerformQuery<ExtendableTableEntry>
                (t => t.PartitionKey == dynamicEntity.PartitionKey && t.RowKey == dynamicEntity.RowKey)
                .SingleOrDefault();

            Assert.IsNotNull(ret);
            Assert.IsNull(ret["stringval"]);
            Assert.IsNull(ret["int32val"]);
        }

        private void DeleteEntityWithKey(string key)
        {
            var tabCtx = Kernel.Get<AzureTableContext>("azureTableCtxTest");
            Assert.IsNotNull(tabCtx);
            tabCtx.Delete<ExtendableTableEntry>(t => t.PartitionKey == key && t.RowKey == key);
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
        public void AzureTableContextPerformSelectQueryReturnsASubsetOfEntity()
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

            var tabCtx = Kernel.Get<AzureTableContext>("MyExtendableTableServiceEntityCtxTest");
            Assert.IsNotNull(tabCtx);

            tabCtx.Store(new List<StorageTableEntryInterface>() { myEntity });
            tabCtx.SaveChanges();

            var tabCtxQuery = Kernel.Get<AzureTableContext>("MyExtendableTableServiceEntityCtxTest");
            Assert.IsNotNull(tabCtxQuery);

            var ret = tabCtxQuery.PerformQuery<MyExtendableTableEntry>
                (t => t.PartitionKey == myEntity.PartitionKey && t.RowKey == myEntity.RowKey)
                .SingleOrDefault();

            Assert.AreEqual(myEntity.RowKey, ret.RowKey);
            Assert.AreEqual(myEntity.PartitionKey, ret.PartitionKey);
            Assert.AreEqual(myEntity.PropertyOne, ret.PropertyOne);
            Assert.AreEqual(myEntity.PropertyTwo, ret.PropertyTwo);

            tabCtxQuery = Kernel.Get<AzureTableContext>("MyExtendableTableServiceEntityCtxTest");

            Expression<Func<MyExtendableTableEntry, bool>> query =
                t => t.PartitionKey == myEntity.PartitionKey && t.RowKey == myEntity.RowKey;

            var partialRet = tabCtxQuery.PerformSelectQuery
                (query, s => new {PropertyOne = s.PropertyOne})
            .SingleOrDefault();

            Assert.IsNotNull(partialRet);
            Assert.AreEqual(myEntity.PropertyOne, partialRet.PropertyOne);
        }

        [Test]
        public void PerformParallelQueriesReturnsAllResultsJoinedTogether()
        {
            var tabCtx = Kernel.Get<AzureTableContext>("MyExtendableTableServiceEntityCtxTest");
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
                tabCtx.Store(new List<StorageTableEntryInterface>() { myEntity });
            }
            tabCtx.SaveChanges();

            var searchQueries = new Expression<Func<MyExtendableTableEntry, bool>>[]
                                    {
                                        entry => entry.PartitionKey == partitions[0],
                                        entry => entry.PartitionKey == partitions[1],
                                        entry => entry.PartitionKey == partitions[2],
                                        entry => entry.PartitionKey == partitions[3]
                                    };

            var ret = tabCtx.PerformParallelQueries(searchQueries);
            Assert.Count(4, ret);
            foreach (var partition in partitions)
            {
                Assert.Count(1, ret.Where(t => t.PartitionKey == partition));
            }
            
        }

        [Test]
        public void PerformParallelSelectQueriesReturnsAllResultsJoinedTogether()
        {
            var tabCtx = Kernel.Get<AzureTableContext>("MyExtendableTableServiceEntityCtxTest");
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
                tabCtx.Store(new List<StorageTableEntryInterface>() { myEntity });
            }
            tabCtx.SaveChanges();

            var searchQueries = new Expression<Func<MyExtendableTableEntry, bool>>[]
                                    {
                                        entry => entry.PartitionKey == partitions[0],
                                        entry => entry.PartitionKey == partitions[1],
                                        entry => entry.PartitionKey == partitions[2],
                                        entry => entry.PartitionKey == partitions[3]
                                    };

            var ret = tabCtx.PerformParallelSelectQueries(searchQueries, s => new { PropertyOne = s.PropertyOne });
            Assert.Count(4, ret);
            for (int i = 0; i < ret.Count(); i++ )
            {
                Assert.Count(1, ret.Where(t => t.PropertyOne == i.ToString(CultureInfo.InvariantCulture)));
            }

        }
    }
}
