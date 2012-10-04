using System.Collections.Generic;
using NUnit.Framework;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using Website.Azure.Common.Binding;
using Website.Infrastructure.Binding;
using Website.Test.Common;

namespace Website.Azure.Common.Tests
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
        }

        private static readonly List<INinjectModule> NinjectModules = new List<INinjectModule>()
                  {
                      new InfrastructureNinjectBinding(),
                      new AzureCommonNinjectBinding()
                  };
    }
}
