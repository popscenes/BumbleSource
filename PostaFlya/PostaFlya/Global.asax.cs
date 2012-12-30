using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.WebPages;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Ninject;
using Ninject.Syntax;
using PostaFlya.App_Start;
using Website.Application.Authentication;
using Website.Application.Command;
using Website.Application.Schedule;
using Website.Application.WebsiteInformation;
using Website.Application.Extension.Validation;
using PostaFlya.Areas.Default;
using PostaFlya.Areas.TaskJob;
using Website.Azure.Common.Environment;
using Website.Common.Environment;
using Website.Common.Extension;
using Website.Common.Filters;
using PostaFlya.Domain.Behaviour;
using Website.Infrastructure.Util;
using Website.Domain.Tag;
using GlobalFilterCollection = System.Web.Mvc.GlobalFilterCollection;

namespace PostaFlya
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapHttpRoute(
                name: "BrowserApi",
                routeTemplate: "api/Browser/{browserid}/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            routes.MapHttpRoute(
                name: "ProfileApiRoute",
                routeTemplate: "api/Profile/{handle}/{controller}/{id}",
                defaults: new { controller = "ProfileApi", id = RouteParameter.Optional }
            );

            routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );



            //bulletin route
            routes.MapRoute(
                name: "Bulletin",
                url: "",
                defaults: new { controller = "Bulletin", action = "Get" }
                );

            routes.MapRoute(
                name: "BulletinDetail",
                url: "{id}",
                defaults: new { controller = "Bulletin", action = "Detail" },
                constraints: new { id = ".+@[0-9]{2}-[a-zA-Z]{3}-[0-9]{2}" }
                );

            //profile view routes
            routes.MapRoute(
                name: "ProfileView",
                url: "{name}",
                defaults: new { controller = "Profile", action = "Get" },
                constraints: new { name = "[0-9a-zA-Z_]+" }
            );

            //default routes
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { id = UrlParameter.Optional }
                );

        }

        public static void Configure(HttpConfiguration config)
        {
            //Web Api doesn't use model validators atm, if this changes in the future no need for this
            config.Filters.Add(new ApiValidationActionFilter());
            //not supported anymore.
            //config.ServiceResolver.SetService(typeof(IHttpControllerFactory), new AreaHttpControllerFactory(GlobalConfiguration.Configuration));
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            Configure(GlobalConfiguration.Configuration); 
            RegisterRoutes(RouteTable.Routes);

            //BundleTable.Bundles.RegisterTemplateBundles();
            //BundleTable.Bundles.EnableDefaultBundles();

            var init = DependencyResolver.Get<InitServiceInterface>(md => md.Has("tablestorageinit"));
            if (init != null)
                init.Init(DependencyResolver);

            init = DependencyResolver.Get<InitServiceInterface>(md => md.Has("storageinit"));
            if (init != null)
                init.Init(DependencyResolver);


            RegisterWebsiteInformation();


            AddSpecifiedDisplayModeProviders();
            RegisterAssetBundles();
            
            ValidationAdapters.Register();

            //not using broadcast communicators for anything atm. Was being used for cache notifications but now using azure caching
            //re-enable if we ever need to communicate across roles
            //RunCommunicatorProcessorQueue();

            //debug only
            //
            //return when renabling worker role
            //if (!AzureEnv.IsRunningInCloud()) //done in a worker role in the cloud
                RunCommandProcessorQueue();

        }

        protected void RegisterWebsiteInformation()
        {
            var websiteInfoService = DependencyResolver.Get<WebsiteInfoServiceInterface>();
            var tags = new Tags(new string[]{ "event","social","comedy","theatre","books","pets","lost","found","services","music","fashion","food & drink","job","task","wanted","for sale","for free","sport","automotive","education","sale","garage","film","art & craft","photography","accommodation","technology","property","kids","politics"});
            var websiteInfo = new WebsiteInfo()
            {
                Tags = tags.ToString(),
                WebsiteName = "postaFlya",
                BehaivoirTags = "postaFlya",
                FacebookAppID = "180085418789276",
                FacebookAppSecret = "a0996e44b6ccb8439e73968dc29fec5c"
            };

            websiteInfoService.RegisterWebsite("127.0.0.1", websiteInfo);
            websiteInfoService.RegisterWebsite("127.0.0.2", websiteInfo);
            websiteInfoService.RegisterWebsite("localhost", websiteInfo);

             websiteInfo = new WebsiteInfo()
            {
                Tags = tags.ToString(),
                WebsiteName = "postaFlya",
                BehaivoirTags = "postaFlya",
                FacebookAppID = "306027489468762",
                FacebookAppSecret = "f765d675dd653fa81e1ee25cfaa27494"
            };
             websiteInfoService.RegisterWebsite("postaflyaprod.cloudapp.net", websiteInfo);
        }



        public override void Init()
        {
            base.Init();
            PostAuthenticateRequest += (s, e) => SetPrincipal();
        }

        /// <summary>
        /// Use ONLY as last resort. When depenecy injection isn't possible
        /// otherwise will fall into the service locator anti-pattern
        /// </summary>
        public static IResolutionRoot DependencyResolver
        {
            get { return NinjectWebCommon.bootstrapper.Kernel; }
        }

        public static void SetPrincipal()
        {
            var context = DependencyResolver.Get<HttpContextBase>();
            if (context == null) return;
            if (context.User == null) return;

            WebIdentity id = new WebIdentity();

            HttpCookie authCookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                string encTicket = authCookie.Value;
                if (!String.IsNullOrEmpty(encTicket))
                {
                    FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(encTicket);
                    id = new WebIdentity(ticket);
                    GenericPrincipal prin = new GenericPrincipal(id, new string[] { "Participant" });

                    HttpContext.Current.User = prin;

                    return;
                }
            }

            GenericPrincipal emptyPrin = new GenericPrincipal(id, null);
            HttpContext.Current.User = emptyPrin;
        }

        public static bool CheckDisplayMode(HttpContextBase context, bool mobile)
        {
            var viewString = mobile ? "mobile" : "desk";
            if (context.Request == null)
                return false;

            if (context.Request.Url != null &&
                context.Request.Url.AbsoluteUri
                .IndexOf("//" + viewString + ".", StringComparison.InvariantCultureIgnoreCase) >= 0)
                return true;

            if (context.Request.Url != null &&
                context.Request.Url.AbsoluteUri
                .IndexOf(viewString + "=true", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                context.Request.Cookies.Remove("display");
                context.Response.Cookies.Add(new HttpCookie("display", viewString));
                return true;
            }

            var httpCookie = context.Request.Cookies["display"];
            if (httpCookie != null && httpCookie.Value == viewString)
                return true;

            return false;
        }

        public class MobileSpecifiedDisplayMode : DefaultDisplayMode
        {
            public MobileSpecifiedDisplayMode()
                : base("Mobile")
            {
                ContextCondition = (context) => CheckDisplayMode(context, true);
            }
        }

        public class DeskSpecifiedDisplayMode : DefaultDisplayMode
        {
            public DeskSpecifiedDisplayMode()
                : base("")
            {
                ContextCondition = (context) => CheckDisplayMode(context, false);
            }
        }

        private void AddSpecifiedDisplayModeProviders()
        {
            DisplayModeProvider.Instance.Modes.Insert(0, new MobileSpecifiedDisplayMode());
            DisplayModeProvider.Instance.Modes.Insert(1, new DeskSpecifiedDisplayMode());
        }

        private void RegisterAssetBundles()
        {
            Views.Shared.Bundles.AddBundles(BundleTable.Bundles);
            Views.Bulletin.Bundles.AddBundles(BundleTable.Bundles);
            Views.Flier.Bundles.AddBundles(BundleTable.Bundles);
            Views.FlierImport.Bundles.AddBundles(BundleTable.Bundles);
            Views.HeatMap.Bundles.AddBundles(BundleTable.Bundles);
            Views.Account.Bundles.AddBundles(BundleTable.Bundles);
            Views.Profile.Bundles.AddBundles(BundleTable.Bundles);
            Views.Profile.BundlesEdit.AddBundles(BundleTable.Bundles);
            Views.Payment.Bundles.AddBundles(BundleTable.Bundles);
           
        }


//broadcast role communitcation not needed atm, was being used for role your own distributed caching
//        private WebBackgroundWorker _communicatorQueueWorker;
//        private void RunCommunicatorProcessorQueue()
//        {
//
//            _communicatorQueueWorker = new WebBackgroundWorker((t)
//                =>
//            {
//                //start afresh if there is an unhandled exception
//                var processor = DependencyResolver.Get<QueuedCommandProcessor>(
//                        ctx => ctx.Has("BroadcastCommunicator"));
//                    
//                processor.Run(t.Token);
//            });
//            _communicatorQueueWorker.Start();
//        }

        //See workerrole.cs and webrole.cs.. this only gets run here out of the cloud 
        private WebBackgroundWorker _commandQueueWorker;
        private WebBackgroundWorker _schedulerWorker;
        private void RunCommandProcessorQueue()
        {
            
            _commandQueueWorker = new WebBackgroundWorker((t)
                =>
                    {
                        //will start afresh if there is an unhandled exception
                        var processor = NinjectWebCommon.bootstrapper.Kernel.Get<QueuedCommandProcessor>(ctx => ctx.Has("workercommandqueue"));
                        processor.Run(t.Token);                   
                    });
            _commandQueueWorker.Start();

            if (AzureEnv.GetInstanceIndex() != 0)//just run scheduler on 1st intance
                return;

            _schedulerWorker = new WebBackgroundWorker((t)
                 =>
            {
                //swill start afresh if there is an unhandled exception
                var processor = NinjectWebCommon.bootstrapper.Kernel.Get<SchedulerInterface>();
                processor.Run(t);
            });
            _schedulerWorker.Start();

        }

    }
}