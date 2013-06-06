using System.Web.Mvc;

namespace PostaFlya.Areas.MobileApi
{
    public class MobileApiAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "MobileApi";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "MobileApi_default",
                "MobileApi/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
