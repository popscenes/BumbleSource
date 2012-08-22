using System.Collections.Generic;
using MbUnit.Framework;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using WebSite.Application.Azure.Binding;
using WebSite.Application.Command;
using WebSite.Application.Content;
using WebSite.Azure.Common.Environment;
using WebSite.Azure.Common.TableStorage;
using WebSite.Test.Common;

namespace WebSite.Application.Azure.Tests
{
    [AssemblyFixture]
    class TestFixtureSetup
    {

        [FixtureSetUp]
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
                      new WebSite.Azure.Common.Binding.AzureCommonNinjectBinding(),
                      new AzureApplicationNinjectBinding(),
                      //new DataRepository.Binding.AzureRepositoryNinjectBinding(c => c.InTransientScope())
                  };
    }
}
