using System.Web.Http;
using System.Web.Mvc;
using PostaFlya.Areas.MobileApi.App_Start;

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
            MobileApiConfig.Register(GlobalConfiguration.Configuration);
        }
    }
}
