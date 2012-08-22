using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using PostaFlya.DataRepository.Binding;
using PostaFlya.DataRepository.Browser;
using PostaFlya.DataRepository.Internal;
using PostaFlya.Domain.Binding;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Comments;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Likes;
using WebSite.Azure.Common.Environment;
using WebSite.Azure.Common.TableStorage;
using WebSite.Infrastructure.Binding;
using WebSite.Test.Common;
using PostaFlya.Mocks.Domain.Defaults;

namespace PostaFlya.DataRepository.Tests
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
                      new GlobalDefaultsNinjectModule(),
                      new WebSite.Azure.Common.Binding.AzureCommonNinjectBinding(),
                      new PostaFlya.DataRepository.Binding.AzureRepositoryNinjectBinding(c => c.InTransientScope()),
                      new PostaFlya.Domain.Binding.DefaultServicesNinjectBinding(),
                      new PostaFlya.Domain.TaskJob.Binding.TaskJobNinjectBinding(),
                      new WebSite.Infrastructure.Binding.InfrastructureNinjectBinding(),
                      new PostaFlya.DataRepository.Binding.TableNameNinjectBinding()
                  };
    }
}
