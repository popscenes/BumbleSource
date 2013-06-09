using System;
using System.Linq;
using NUnit.Framework;
using Ninject;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;

namespace Website.Azure.Common.Tests.TableStorage
{
    [TestFixture("dev")]
    [TestFixture("real")]
    public class ExtendableTableEntryTests
    {

        static StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        public ExtendableTableEntryTests(string env)
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
            tableNameAndPartitionProviderService.Add<ExtendableTableEntry>("testExtendableTable", entity => entity.PartitionKey, entity => entity.RowKey);

            var context = Kernel.Get<TableContextInterface>();

            foreach (var tableName in tableNameAndPartitionProviderService.GetAllTableNames())
            {
                context.InitTable<ExtendableTableEntry>(tableName);
            }

            context.Delete<ExtendableTableEntry>("testExtendableTable", null);
            context.SaveChanges();
        }


        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<TableNameAndIndexProviderServiceInterface>();
            Kernel.Unbind<TableContextInterface>();
            AzureEnv.UseRealStorage = false;
        }


        [Test]
        public void ExtendableTableEntrySerialize()
        {
            var tableEntry = new ExtendableTableEntry();
            tableEntry.PartitionKey = Guid.NewGuid().ToString();
            tableEntry.RowKey = Guid.NewGuid().ToString();

            FillPropertyGroupWithEdmTypes(tableEntry);

            var nameAndPartitionProviderService = Kernel.Get<TableNameAndIndexProviderServiceInterface>();
            var tableName = nameAndPartitionProviderService.GetTableName<ExtendableTableEntry>();
            var tabCtx = Kernel.Get<TableContextInterface>();
            tabCtx.Store(tableName, tableEntry);
            tabCtx.SaveChanges();

            var tabCtxRet = Kernel.Get<TableContextInterface>();
            var ret = tabCtxRet.PerformQuery<ExtendableTableEntry>(tableName, query: e => e.PartitionKey == tableEntry.PartitionKey
                                                                                          && e.RowKey == tableEntry.RowKey)
                                                                   .SingleOrDefault();

            Assert.IsNotNull(ret);
            var deserialized = ret.GetEntity<ExtendableTableEntry>();

            AssertEdmTypes(deserialized);
        }

        private void FillPropertyGroupWithEdmTypes(ExtendableTableEntry entity)
        {
            //un supported types converting to supported types in serialization
            //"Edm.String"
            entity["stringval"] = "1234";
            //"Edm.Byte"
            entity["byteval"] = (byte)1;
            //"Edm.SByte"
            entity["sbyteval"] = (sbyte)-1;
            //"Edm.Int16"
            entity["int16val"] = (short)4500;
            //"Edm.Single"
            entity["singleval"] = (float)4500.45;
            //"Edm.Decimal" 
            entity["decimalval"] = (decimal)4500.45;

            //"Edm.Int32"
            entity["int32val"] = (int)4500;
            //"Edm.Int64"
            entity["int64val"] = (long)4500;
            //"Edm.Double"
            entity["doubleval"] = (double) 4500.45;   
            //"Edm.Boolean"
            entity["boolval"] = true;
            //"Edm.DateTime"
            entity["datetimeval"] = new DateTime(2001, 1, 1);
            //"Edm.Binary"
            entity["binval"] = new byte[] { 1, 2, 3 };
            //"Edm.Guid"
            entity["guidval"] = new Guid("871531C8-52DD-4A97-8508-37A84E63DD35");
        }

        private void AssertEdmTypes(ExtendableTableEntry entry)
        {

            //un supported types converting to supported types in serialization
            //"Edm.String"
            Assert.AreEqual("1234", entry["stringval"]);
            //"Edm.Byte"
            Assert.AreEqual((byte)1, Convert.ToByte(entry["byteval"]));
            //"Edm.SByte"
            Assert.AreEqual((sbyte)-1, Convert.ToSByte(entry["sbyteval"]));
            //"Edm.Int16"
            Assert.AreEqual((short)4500, Convert.ToInt16(entry["int16val"]));
            //"Edm.Single"
            Assert.AreEqual((float)4500.45, Convert.ToSingle(entry["singleval"]));
            //"Edm.Decimal" 
            Assert.AreEqual((decimal)4500.45, Convert.ToDecimal(entry["decimalval"]));

            //"Edm.Int32"
            Assert.AreEqual((int)4500, entry["int32val"]);
            //"Edm.Int64"
            Assert.AreEqual((long)4500, Convert.ToInt64(entry["int64val"]));
            //"Edm.Double"
            Assert.AreEqual((double)4500.45, entry["doubleval"]);
            //"Edm.Boolean"
            Assert.AreEqual(true, entry["boolval"]);
            //"Edm.DateTime"
            Assert.AreEqual(new DateTime(2001, 1, 1), entry["datetimeval"]);
            //"Edm.Binary"
            Assert.AreEqual(new byte[] { 1, 2, 3 }, entry["binval"]);
            //"Edm.Guid"
            Assert.AreEqual(new Guid("871531C8-52DD-4A97-8508-37A84E63DD35"), entry["guidval"]);
        }
    }
}
