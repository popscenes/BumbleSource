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
using WebSite.Infrastructure.Binding;
using WebSite.Test.Common;

namespace WebSite.Azure.Common.Tests
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
                      new WebSite.Azure.Common.Binding.AzureCommonNinjectBinding()
                  };
    }
}
