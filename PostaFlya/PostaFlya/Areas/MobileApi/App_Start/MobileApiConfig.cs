using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace PostaFlya.Areas.MobileApi.App_Start
{
    public static class MobileApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "GigsNearBy",
                routeTemplate: "mobileapi/gigs/near",
                defaults: new { Controller = "FlyersByLocation" }
                );

                config.Routes.MapHttpRoute(
                name: "GigsLatest",
                routeTemplate: "mobileapi/gigs/latest",
                defaults: new { Controller = "FlyersByLatest" }
                );          
        }
    }
}