using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using PostaFlya.Domain.Behaviour;

namespace PostaFlya.Areas.TaskJob
{
    public class TaskJobAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "TaskJob";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {


        }
    }
}
