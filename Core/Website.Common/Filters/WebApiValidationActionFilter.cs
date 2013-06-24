using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using Website.Infrastructure.Command;

namespace Website.Common.Filters
{
    //Web Api doesn't use model validators atm, if this changes in the future no need for this
    public abstract class ApiValidationActionFilterBaseAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext context)
        {
            var modelState = context.ModelState;
            if (modelState.IsValid) return;

            var faultMessage = CreateFaultMessage(context.ModelState);
            var responseMessage = context.Request.CreateResponse(HttpStatusCode.BadRequest, faultMessage);
            context.Response = responseMessage;
        }

        protected abstract object CreateFaultMessage(IDictionary<string, ModelState> modelState);


    }

    //Web Api doesn't use model validators atm, if this changes in the future no need for this
    public class WebApiValidationActionFilter : ApiValidationActionFilterBaseAttribute
    {
        protected override object CreateFaultMessage(IDictionary<string, ModelState> modelState)
        {
            var errList = new List<MsgResponseDetail>();
            var errorResponse = new MsgResponse {Message = "Validation Error", Details = errList, IsError = true};

            errList.AddRange(from key in modelState.Keys
                             let state = modelState[key]
                             where state.Errors.Any()
                             select new MsgResponseDetail()
                                        {
                                            Property = key, Message = 
                                                state.Errors
                                                    .First(e => !string.IsNullOrWhiteSpace(e.ErrorMessage))
                                                    .ErrorMessage
                                        });
            return errorResponse;
        }
    }
}