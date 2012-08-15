using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Routing;

namespace PostaFlya.Helpers
{
    public static class UrlHelperExtensions
    {
        //going straight to blob storage or CDN not through controller
//        public static string GetImgUrlFmt(this UrlHelper url)
//        {
//            if (url == null)//in testing we don't care
//                return "{0}";
//
//            var imageUrlFmt = url.Route("Default", new { Controller = "Img", Action = "Get", id = "123" });
//            imageUrlFmt = imageUrlFmt.Replace("123", "{0}");
//            //dunno why but route adds ?httproute=True don't need it
//            var stripArg = 0;
//            if ((stripArg = imageUrlFmt.IndexOf("?httproute=", StringComparison.InvariantCultureIgnoreCase)) >= 0)
//                imageUrlFmt = imageUrlFmt.Substring(0, stripArg);
//            return imageUrlFmt;
//        }
    }
}