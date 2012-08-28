using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using MbUnit.Framework;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using Website.Application.Caching.Command;
using PostaFlya.Domain.Binding;
using Website.Infrastructure.Binding;
using Website.Test.Common;
using PostaFlya.Mocks.Domain.Data;
using Website.Mocks.Domain.Defaults;

namespace PostaFlya.Application.Domain.Tests
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
            CurrIocKernel.Bind<CacheNotifier>().ToMethod(context => 
                new CacheNotifier(null, false));//just don't use notifier
        }

        private static readonly List<INinjectModule> NinjectModules = new List<INinjectModule>()
                  {
                      new GlobalDefaultsNinjectModule(),
                      new InfrastructureNinjectBinding(),
                      new DefaultServicesNinjectBinding(),
                      new Website.Domain.Binding.DefaultServicesNinjectBinding(),
                      new CommandNinjectBinding(),
                      new TestRepositoriesNinjectModule(),
                      new Website.Mocks.Domain.Data.TestRepositoriesNinjectModule(),
                      new PostaFlya.Domain.TaskJob.Binding.TaskJobNinjectBinding(),
                  };
    }
}
