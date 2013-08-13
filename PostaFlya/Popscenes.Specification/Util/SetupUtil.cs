using System.Runtime.Caching;
using System.Web.Http;
using NUnit.Framework;
using Ninject;
using Popscenes.Specification.Util;
using PostaFlya.App_Start;
using PostaFlya.Areas.MobileApi.App_Start;
using PostaFlya.Binding;
using TechTalk.SpecFlow;
using Website.Application.Messaging;
using Website.Test.Common;

namespace Popscenes.Specification.Util
{
  
    public static class SetupUtil
    {
        public static void CreateServer(IKernel kernel)
        {
            // Server
            var config = new HttpConfiguration();

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            kernel.Load(AllWebSiteBindings.NinjectModules);

            config.DependencyResolver = new TestNinjectHttpDependencyResolver(kernel, config);

            WebApiConfig.Register(config);
            MobileApiConfig.Register(config);

            var server = new HttpServer(config);

            SpecUtil.Server = server; 
            SpecUtil.Kernel = kernel;
            ScenarioContext.Current[SpecUtil.BaseAddressContext] = "http://popscenes.com/";
        }

        public static void SetupMockServices(StandardKernel kernel)
        {
            kernel.Rebind<ObjectCache>()
                .ToMethod(ctx => new TestSerializingCache())
                .InSingletonScope();

            kernel.Get<MessageQueueFactoryInterface>()
                .Delete("workercommandqueue");
        }
    }

    public class SetupBase
    {
        public SetupBase()
        {

        }

        public virtual void BeforeScenario()
        {
            var kernel = new StandardKernel();
            kernel.Settings.InjectParentPrivateProperties = true;
            kernel.Settings.InjectNonPublic = true;
            SetupUtil.CreateServer(kernel);
            StorageUtil.InitTableStorage();
            SetupUtil.SetupMockServices(kernel);        
        }

        public virtual void AfterScenario()
        {
            SpecUtil.Kernel.Dispose();
        }

        public virtual void AfterStep()
        {
            StorageUtil.ProcessAllMessagesAndEvents();
        }
    }
}


