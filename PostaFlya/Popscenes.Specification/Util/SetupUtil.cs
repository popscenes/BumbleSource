using System.Web.Http;
using Ninject;
using PostaFlya.App_Start;
using PostaFlya.Binding;
using TechTalk.SpecFlow;

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

            var server = new HttpServer(config);

            SpecUtil.Server = server; 
            SpecUtil.Kernel = kernel;
            ScenarioContext.Current[SpecUtil.BaseAddressContext] = "http://popscenes.com/";
        }



        public static void SetupMockServices(StandardKernel kernel)
        {

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
            SetupUtil.SetupMockServices(kernel);
        }

        public virtual void AfterScenario()
        {
            SpecUtil.Kernel.Dispose();
        }

        public virtual void AfterStep()
        {

        }
    }

}
