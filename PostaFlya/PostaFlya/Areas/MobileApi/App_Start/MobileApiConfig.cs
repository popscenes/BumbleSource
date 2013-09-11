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
                name: "GigsByDate",
                routeTemplate: "mobileapi/gigs/bydate",
                defaults: new { Controller = "FlyersByLocation" }
                );

            config.Routes.MapHttpRoute(
                name: "GigDetail",
                routeTemplate: "mobileapi/gigs/{id}",
                defaults: new { Controller = "FlyerDetail" }
                );


            config.Routes.MapHttpRoute(
                name: "GigsLatest",
                routeTemplate: "mobileapi/gigs",
                defaults: new { Controller = "FlyersByFeatured" }
                );


            config.Routes.MapHttpRoute(
                name: "BoardGigs",
                routeTemplate: "mobileapi/boards/{BoardId}/gigs",
                defaults: new { Controller = "FlyersByBoard" }
                );

        }
    }
}