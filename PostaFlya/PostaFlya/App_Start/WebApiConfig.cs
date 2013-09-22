using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Website.Azure.Common.Environment;
using Website.Common.Filters;
using Website.Common.HttpMessageHandlers;
using Website.Common.MediaFormatters;

namespace PostaFlya.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            if(AzureEnv.IsRunningInDevFabric())
                config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            
            //Web Api doesn't use model validators atm, if this changes in the future no need for this
            RegisterMediaFormatters.For(config);

            RegisterGlobalMessageHandlers.For(config);


            RegisterRoutes(config.Routes);
        }

        private static void RegisterRoutes(HttpRouteCollection routes)
        {
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
        }
    }
}