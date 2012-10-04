using System.Collections.Generic;
using Microsoft.WindowsAzure.StorageClient;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using Website.Application.Azure.Binding;
using Website.Application.Command;
using Website.Application.Content;
using Website.Azure.Common.Binding;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;
using Website.Test.Common;

namespace Website.Application.Azure.Tests
{
    [SetUpFixture]
    class TestFixtureSetup
    {

        [SetUp]
        public void FixtureSetup()
        {
            Assert.IsNotNull(CurrIocKernel);
            InitializeBinding();
        }


        private static MoqMockingKernel _currIocKernel;
        public static MoqMockingKernel CurrIocKernel
        {
            get
            {
                if (_currIocKernel == null)
                {
                    var testKernel = new TestKernel(NinjectModules);
                    _currIocKernel = testKernel.Kernel;              
                }
                return _currIocKernel;
            }
        }

        private static void InitializeBinding()
        {
            CurrIocKernel.Unbind<BlobStorageInterface>();
            CurrIocKernel.Unbind<QueueInterface>();
            CurrIocKernel.Unbind<MessageFactoryInterface>();
            CurrIocKernel.Unbind<CloudQueue>();
            CurrIocKernel.Unbind<CloudBlobContainer>();

            var tableNameProv = CurrIocKernel.Get<TableNameAndPartitionProviderServiceInterface>();
            tableNameProv.SuffixTableNames("test");

            AzureEnv.UseRealStorage = true;
            var tctx = CurrIocKernel.Get<TableContextInterface>();
            foreach (var tableName in tableNameProv.GetAllTableNames())
            {
                tctx.InitTable<JsonTableEntry>(tableName);
                tctx.Delete<JsonTableEntry>(tableName, null);
            }
            AzureEnv.UseRealStorage = false;
            tctx = CurrIocKernel.Get<TableContextInterface>();
            foreach (var tableName in tableNameProv.GetAllTableNames())
            {
                tctx.InitTable<JsonTableEntry>(tableName);
                tctx.Delete<JsonTableEntry>(tableName, null);
            }
        }

        private static readonly List<INinjectModule> NinjectModules = new List<INinjectModule>()
                  {
                      new AzureCommonNinjectBinding(),
                      new AzureApplicationNinjectBinding(),
                      //new DataRepository.Binding.AzureRepositoryNinjectBinding(c => c.InTransientScope())
                  };
    }
}
