using System.Diagnostics;
using System.Web.Http;
using Website.Azure.Common.Environment;
using PostaFlya.Binding;
using Website.Common.Binding;
using Website.Common.Filters;

[assembly: WebActivator.PreApplicationStartMethod(typeof(PostaFlya.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(PostaFlya.App_Start.NinjectWebCommon), "Stop")]

namespace PostaFlya.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    public static class NinjectWebCommon 
    {
        public static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            //Debugger.Break();
            Trace.TraceInformation("Ninject start " + AzureEnv.GetIdForInstance());
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            Trace.TraceInformation("Ninject stop " + AzureEnv.GetIdForInstance());
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            RegisterServices(kernel);
            //until a new version of ninject that supports web api
            GlobalConfiguration.Configuration.DependencyResolver = new NinjectHttpDependencyResolver(kernel);
            //this means that properties in filter attributes can be bound for http controllers
            kernel.Bind<System.Web.Http.Filters.IFilterProvider>().To<DefaultNinjectHttpFilterProvider>();
            //end until a new version
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        public static void RegisterServices(IKernel kernel)
        {
            Trace.TraceInformation("Start Ninject load modules " + AzureEnv.GetIdForInstance());

            kernel.Load(AllWebSiteBindings.NinjectModules);

            Trace.TraceInformation("Ninject load modules complete " + AzureEnv.GetIdForInstance() + ":" + AllWebSiteBindings.NinjectModules);
        }        
    }
}
