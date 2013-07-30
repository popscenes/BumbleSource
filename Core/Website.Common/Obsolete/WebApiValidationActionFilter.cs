using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using Website.Infrastructure.Command;

namespace Website.Common.Obsolete
{
    [Obsolete("See ApiValidationActionFilter")]
    public class WebApiValidationActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext context)
        {
            var modelState = context.ModelState;
            if (modelState.IsValid) return;

            var faultMessage = CreateFaultMessage(context.ModelState);
            var responseMessage = context.Request.CreateResponse(HttpStatusCode.BadRequest, faultMessage);
            context.Response = responseMessage;
        }

        protected MsgResponse CreateFaultMessage(IDictionary<string, ModelState> modelState)
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