using System.Collections.Generic;
using System.Data.Services.Client;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;
using Website.Test.Common;
using Website.Mocks.Domain.Defaults;

namespace PostaFlya.DataRepository.Tests
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
            var tableNameProv = CurrIocKernel.Get<TableNameAndPartitionProviderServiceInterface>();

            tableNameProv.SuffixTableNames("test");

            //mmm moving away from testing repos, now that json repo is the primary
            //store there is no need to test every repo, only specific query service functionality
//            AzureEnv.UseRealStorage = true;
//            var tctx = CurrIocKernel.Get<TableContextInterface>();
//            foreach (var tableName in tableNameProv.GetAllTableNames())
//            {
//                tctx.InitTable<JsonTableEntry>(tableName);
//                tctx.Delete<JsonTableEntry>(tableName, null);
//            }
            AzureEnv.UseRealStorage = false;
            var tctx = CurrIocKernel.Get<TableContextInterface>();
            foreach (var tableName in tableNameProv.GetAllTableNames())
            {
                tctx.InitTable<JsonTableEntry>(tableName);
                tctx.Delete<JsonTableEntry>(tableName, null);
            }
            tctx.SaveChanges(SaveChangesOptions.ReplaceOnUpdate);
        }

        private static readonly List<INinjectModule> NinjectModules = new List<INinjectModule>()
                  {
                      new GlobalDefaultsNinjectModule(),
                      new Website.Azure.Common.Binding.AzureCommonNinjectBinding(),
                      new PostaFlya.DataRepository.Binding.AzureRepositoryNinjectBinding(c => c.InTransientScope()),
                      new PostaFlya.Domain.Binding.DefaultServicesNinjectBinding(),
                      new PostaFlya.Domain.TaskJob.Binding.TaskJobNinjectBinding(),
                      new Website.Infrastructure.Binding.InfrastructureNinjectBinding(),
                      new PostaFlya.DataRepository.Binding.TableNameNinjectBinding()
                  };
    }
}
