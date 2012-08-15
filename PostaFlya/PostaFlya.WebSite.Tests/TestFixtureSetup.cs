using System.Collections.Generic;
using MbUnit.Framework;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using PostaFlya.Domain.Binding;
using PostaFlya.Mocks.Domain.Data;
using PostaFlya.Mocks.Domain.Defaults;
using WebSite.Test.Common;
using WebSite.Infrastructure.Binding;
//using PostaFlya.Mocks.Domain.Data;
//using PostaFlya.Mocks.Domain.Defaults;

namespace PostaFlya.WebSite.Tests
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

        }

        private static readonly List<INinjectModule> NinjectModules = new List<INinjectModule>()
                  {
                    new GlobalDefaultsNinjectModule(),
                    new DefaultServicesNinjectBinding(),
                    new InfrastructureNinjectBinding(),
                    new Domain.Binding.CommandNinjectBinding(),
                    new TestRepositoriesNinjectModule()
                  };
    }
}
