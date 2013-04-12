using System;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.WebPages;
using Ninject;
using Ninject.Syntax;
using PostaFlya.App_Start;
using PostaFlya.Controllers;
using Website.Application.Authentication;
using Website.Application.Command;
using Website.Application.Domain.TinyUrl.Web;
using Website.Application.Schedule;
using Website.Application.WebsiteInformation;
using Website.Application.Extension.Validation;
using Website.Azure.Common.Environment;
using Website.Common.Environment;
using Website.Common.Filters;
using Website.Common.Util;
using Website.Infrastructure.Configuration;
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
            filters.Add(new RemoveWwwRedirectFilter());
        }


        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(name: "FourOhFour", url: "FourOhFour/{id}",
                            defaults: new {controller = "Error", action = "FourOhFour", id = UrlParameter.Optional});

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

            
            //sitemap
            routes.MapRoute(
                name: "SiteMap",
                url: "{sitemap}",
                defaults: new { controller = "SiteMap", action = "Index"},
                constraints: new { sitemap = @"sitemap[0-9]*\.xml" }
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
                constraints: new { id = "[-0-9a-zA-Z]+@[0-9]{2}-[a-zA-Z]{3}-[0-9]{2}" }
                );

            //tiny url route
            routes.MapRoute(
                name: "FlierTinyUrl",
                url: "{path}",
                defaults: new { controller = "TinyUrl", action = "Get" },
                constraints: new { path = DependencyResolver.Current.GetService<TinyUrlRouteConstraint>()}
            );

            //static routes
            routes.MapRoute(
                name: "TermsOfService",
                url: "termsofservice",
                defaults: new { controller = "Static", action = "TermsOfService" }
                );
            routes.MapRoute(
                name: "PrivacyPolicy",
                url: "privacypolicy",
                defaults: new { controller = "Static", action = "PrivacyPolicy" }
                );
            
            //default routes
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { id = UrlParameter.Optional }
                );

            //routes.MapRoute(name: "FourOhFour", url: "{*allMissed}",
            //                defaults: new { controller = "Error", action = "FourOhFour", id = UrlParameter.Optional });


        }

        public static void UpdateScriptsAndStylesForCdn(ConfigurationServiceInterface config)
        {
            var cdn = config.GetSetting("SiteCdnUrl");
            if(string.IsNullOrWhiteSpace(cdn))
                return;

            Scripts.DefaultTagFormat = Scripts.DefaultTagFormat.Replace("src=\"", "src=\"" + cdn);
            Styles.DefaultTagFormat = Styles.DefaultTagFormat.Replace("href=\"", "href=\"" + cdn);

        }
        public static void Configure(HttpConfiguration config)
        {
            //Web Api doesn't use model validators atm, if this changes in the future no need for this
            config.Filters.Add(new ApiValidationActionFilter());
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            Configure(GlobalConfiguration.Configuration); 
            RegisterRoutes(RouteTable.Routes);

            //BundleTable.Bundles.RegisterTemplateBundles();
            //BundleTable.Bundles.EnableDefaultBundles();

            var init = NinjectDependencyResolver.Get<InitServiceInterface>(md => md.Has("tablestorageinit"));
            if (init != null)
                init.Init(NinjectDependencyResolver);

            init = NinjectDependencyResolver.Get<InitServiceInterface>(md => md.Has("storageinit"));
            if (init != null)
                init.Init(NinjectDependencyResolver);


            RegisterWebsiteInformation();


            AddSpecifiedDisplayModeProviders();
            RegisterAssetBundles();
            
            ValidationAdapters.Register();

            UpdateScriptsAndStylesForCdn(NinjectDependencyResolver.Get<ConfigurationServiceInterface>());

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
            var websiteInfoService = NinjectDependencyResolver.Get<WebsiteInfoServiceInterface>();
            var config = NinjectDependencyResolver.Get<ConfigurationServiceInterface>();

            var tags = new Tags(config.GetSetting("Tags"));
            var websiteInfo = new WebsiteInfo()
            {
                Tags = tags.ToString(),
                WebsiteName = "Popscenes",
                BehaivoirTags = "Popscenes",
                FacebookAppID = "180085418789276",
                FacebookAppSecret = "a0996e44b6ccb8439e73968dc29fec5c"
            };

            websiteInfoService.RegisterWebsite("127.0.0.1", websiteInfo);
            websiteInfoService.RegisterWebsite("127.0.0.2", websiteInfo);
            websiteInfoService.RegisterWebsite("localhost", websiteInfo);
            websiteInfoService.RegisterWebsite("10.0.0.3", websiteInfo);
            websiteInfoService.RegisterWebsite("10.0.0.4", websiteInfo);


             websiteInfo = new WebsiteInfo()
            {
                Tags = tags.ToString(),
                WebsiteName = "Popscenes",
                BehaivoirTags = "Popscenes",
                FacebookAppID = "306027489468762",
                FacebookAppSecret = "f765d675dd653fa81e1ee25cfaa27494"
            };
             websiteInfoService.RegisterWebsite("postaflyaprod.cloudapp.net", websiteInfo);

             websiteInfo = new WebsiteInfo()
             {
                 Tags = tags.ToString(),
                 WebsiteName = "Popscenes",
                 BehaivoirTags = "Popscenes",
                 FacebookAppID = "171501919670169",
                 FacebookAppSecret = "86a86b4edc1bfafbd22e9100532e5e55"
             };
             websiteInfoService.RegisterWebsite(UriUtil.GetCoreDomain(config.GetSetting("SiteUrl")), websiteInfo, true);
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
        public static IResolutionRoot NinjectDependencyResolver
        {
            get { return NinjectWebCommon.bootstrapper.Kernel; }
        }

        public static void SetPrincipal()
        {
            var context = NinjectDependencyResolver.Get<HttpContextBase>();
            if (context == null) return;
            if (context.User == null) return;

            var id = new WebIdentity();

            var authCookie = context.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie != null)
            {
                var encTicket = authCookie.Value;
                if (!String.IsNullOrEmpty(encTicket))
                {
                    var ticket = FormsAuthentication.Decrypt(encTicket);
                    id = new WebIdentity(ticket);
                    //reset in BrowserInformation, if needed
                    var prin = new GenericPrincipal(id, new[] { Website.Domain.Browser.Role.Temporary.ToString() });
                    context.User = prin;
                    return;
                }
            }

            var appLogin = context.Request.Headers.Get(HttpRequestHeader.Authorization.ToString());
            if (!string.IsNullOrWhiteSpace(appLogin))
            {
                id = new WebIdentity(appLogin);
                //reset in BrowserInformation, if needed
                var prin = new GenericPrincipal(id, new[] { Website.Domain.Browser.Role.Temporary.ToString() });
                context.User = prin;
                return;
            }

            var emptyPrin = new GenericPrincipal(id, null);
            context.User = emptyPrin;
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

//        public class MobileSpecifiedDisplayMode : DefaultDisplayMode
//        {
//            public MobileSpecifiedDisplayMode()
//                : base("Mobile")
//            {
//                ContextCondition = (context) => CheckDisplayMode(context, true);
//            }
//        }
//
//        public class DeskSpecifiedDisplayMode : DefaultDisplayMode
//        {
//            public DeskSpecifiedDisplayMode()
//                : base("")
//            {
//                ContextCondition = (context) => CheckDisplayMode(context, false);
//            }
//        }

        private void AddSpecifiedDisplayModeProviders()
        {
            //DisplayModeProvider.Instance.Modes.Insert(0, new MobileSpecifiedDisplayMode());
            //DisplayModeProvider.Instance.Modes.Insert(1, new DeskSpecifiedDisplayMode());
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
//                var processor = NinjectDependencyResolver.Get<QueuedCommandProcessor>(
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
                processor.SchedulerIdentifier = AzureEnv.GetIdForInstance();
                processor.Run(t);
            });
            _schedulerWorker.Start();

        }

    }
}