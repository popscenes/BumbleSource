using System.Web.Http;

namespace PostaFlya.Areas.WebApi.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "WebGigsByDate",
                routeTemplate: "webapi/gigs/bydate",
                defaults: new { Controller = "WebFlyersByLocation" }
                );

            config.Routes.MapHttpRoute(
                name: "WebGigsLatest",
                routeTemplate: "webapi/gigs",
                defaults: new { Controller = "WebFlyersByFeatured" }
                );

            config.Routes.MapHttpRoute(
                name: "WebGigDetail",
                routeTemplate: "webapi/gig/{id}",
                defaults: new { Controller = "WebFlyerDetail" }
                );

            config.Routes.MapHttpRoute(
                name: "WebBoardGigs",
                routeTemplate: "webapi/board/{BoardId}/gigs",
                defaults: new { Controller = "WebFlyersByBoard" }
                );

            config.Routes.MapHttpRoute(
                name: "WebBrowserBoards",
                routeTemplate: "webapi/browser/{BrowserId}/boards",
                defaults: new { Controller = "WebBrowserBoards" }
                );

        }
    }
}