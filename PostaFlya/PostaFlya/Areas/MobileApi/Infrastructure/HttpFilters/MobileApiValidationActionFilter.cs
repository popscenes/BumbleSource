using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.ModelBinding;
using PostaFlya.Areas.MobileApi.Infrastructure.Model;
using Website.Common.Filters;

namespace PostaFlya.Areas.MobileApi.Infrastructure.HttpFilters
{

    public class MobileApiValidationActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext context)
        {
            var modelState = context.ModelState;
            if (modelState.IsValid) return;

            var faultMessage = CreateFaultMessage(context.ModelState);
            var responseMessage = context.Request.CreateResponse(HttpStatusCode.BadRequest, faultMessage);
            context.Response = responseMessage;
        }


        protected ResponseContent<ValidationErrorModel> CreateFaultMessage(IDictionary<string, ModelState> modelState)
        {
            var errorResponse = new ValidationErrorModel
            {
                Errors = (from key in modelState.Keys
                          let state = modelState[key]
                          where state.Errors.Any()
                          let firstOrDefault = state.Errors.FirstOrDefault(
                                  e => !string.IsNullOrWhiteSpace(e.ErrorMessage))
                          where firstOrDefault != null
                          select new ValidationErrorModel.ValidationErrorEntry()
                          {
                              Property = key,
                              Message =
                                  firstOrDefault
                                  .ErrorMessage
                          }).ToList()
            };

            return ResponseContent<ValidationErrorModel>.GetResponse(errorResponse, ResponseContent.StatusEnum.ValidationError);
        }
    }

}