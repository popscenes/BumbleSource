using System.Web.Mvc;
using Website.Common.Util;

namespace PostaFlya
{
    public class RemoveWwwRedirectFilter : System.Web.Mvc.ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var uri = filterContext.HttpContext.Request.Url;
            if (!uri.IsWwwSubDomain()) return;
            
            filterContext.Result = new RedirectResult(uri.RemoveWww().ToString(), permanent: true);
        }
    }
}