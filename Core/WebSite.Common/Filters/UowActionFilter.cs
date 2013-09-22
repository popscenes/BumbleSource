using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using Ninject;
using Ninject.Syntax;
using Website.Infrastructure.Command;

namespace Website.Common.Filters
{
    public class UowActionFilter : System.Web.Mvc.ActionFilterAttribute
    {
        [Inject]
        public UnitOfWorkFactoryInterface UnitOfWorkFactory { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            UnitOfWorkFactory.GetUowInContext().Begin();
            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            UnitOfWorkFactory.GetUowInContext().End();

        }
    }
}