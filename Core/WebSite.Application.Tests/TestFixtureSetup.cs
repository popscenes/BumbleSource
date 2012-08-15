using System.Collections.Generic;
using MbUnit.Framework;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using WebSite.Test.Common;

namespace WebSite.Application.Tests
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
                      new WebSite.Infrastructure.Binding.InfrastructureNinjectBinding(),
                      new WebSite.Application.Binding.ApplicationNinjectBinding()
                  };
    }
}
