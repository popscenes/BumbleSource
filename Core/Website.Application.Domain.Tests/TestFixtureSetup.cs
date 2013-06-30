using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using Website.Application.Binding;
using Website.Application.Caching.Command;
using Website.Domain.Content;
using Website.Infrastructure.Binding;
using Website.Mocks.Domain.Binding;
using Website.Test.Common;
using Website.Domain.Binding;
using Website.Mocks.Domain.Data;
using Website.Mocks.Domain.Defaults;

namespace Website.Application.Domain.Tests
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
            CurrIocKernel.Bind<CacheNotifier>().ToMethod(context => 
                new CacheNotifier(null, false));//just don't use notifier

            CurrIocKernel.BindMockDomainQueryHandlersForTypesFrom(syntax => syntax.InTransientScope()
                    , Assembly.GetAssembly(typeof (Image)));
        }

        private static readonly List<INinjectModule> NinjectModules = new List<INinjectModule>()
                  {
                      new GlobalDefaultsNinjectModule(),
                      new InfrastructureNinjectBinding(),
                      new DefaultServicesNinjectBinding(),
                      new CommandNinjectBinding(),
                      new ApplicationCommandHandlersNinjectBinding(),
                      new TestRepositoriesNinjectModule(),
                      new Website.Mocks.Domain.Binding.QueryHandlerBinding(),
                  };
    }
}
