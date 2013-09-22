using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Ninject;
using Ninject.Syntax;
using Website.Infrastructure.Command;

namespace Website.Common.Filters
{
    public class UowActionFilter : ActionFilterAttribute
    {
        [Inject]
        public IResolutionRoot ResolutionRoot { get; set; }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            ResolutionRoot.Get<UnitOfWorkInterface>().Begin();
            base.OnActionExecuting(actionContext);
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            ResolutionRoot.Get<UnitOfWorkInterface>().End();

        }
    }
}