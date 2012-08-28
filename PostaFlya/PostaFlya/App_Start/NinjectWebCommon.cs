using System.Diagnostics;
using System.Web.Http;
using Website.Azure.Common.Environment;
using PostaFlya.Binding;

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
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
            
            RegisterServices(kernel);

            //until a new version of ninject.webapi
            GlobalConfiguration.Configuration.DependencyResolver = new NinjectDependencyResolver(kernel);

            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            //seems claim both mvc + wepapi ninject extensions bind HttpContext cause double binding errors
            //just rebind
//            kernel.Unbind<HttpContext>();
//            kernel.Unbind<HttpContextBase>();
//            kernel.Rebind<HttpContext>().ToMethod(ctx => HttpContext.Current).InRequestScope();
//            kernel.Rebind<HttpContextBase>().ToMethod(ctx => new HttpContextWrapper(HttpContext.Current)).InTransientScope();
            Trace.TraceInformation("Ninject load modules " + AzureEnv.GetIdForInstance());
            System.Diagnostics.Trace.WriteLine("Ninject load modules trace writeln");
            Trace.TraceError("Ninject load modules trace err");
            Trace.TraceWarning("Ninject load modules trace warn");
            Trace.TraceInformation("Ninject load modules trace info");

            kernel.Load(AllBindings.NinjectModules);

            Trace.TraceInformation("Ninject load modules " + AzureEnv.GetIdForInstance());
            System.Diagnostics.Trace.WriteLine("Ninject load modules trace writeln");
            Trace.TraceError("Ninject load modules trace err");
            Trace.TraceWarning("Ninject load modules trace warn");
            Trace.TraceInformation("Ninject load modules trace info");

            Trace.TraceInformation("Ninject load modules complete " + AzureEnv.GetIdForInstance() + ":" + AllBindings.NinjectModules);
        }        
    }
}
