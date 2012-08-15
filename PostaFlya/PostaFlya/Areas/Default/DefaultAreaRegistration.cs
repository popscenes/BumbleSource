using System.Web.Mvc;
using System.Web.Routing;
using PostaFlya.Domain.Behaviour;

namespace PostaFlya.Areas.Default
{
    public class DefaultAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Default";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {


        }
    }
}
