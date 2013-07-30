using System.Web.Http;
using System.Web.Mvc;
using PostaFlya.Areas.WebApi.App_Start;

namespace PostaFlya.Areas.WebApi
{
    public class WebApiAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "WebApi";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            WebApiConfig.Register(GlobalConfiguration.Configuration);

        }
    }
}
