using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;
using PostaFlya.Areas.MobileApi.Infrastructure.Model;
using Website.Common.Filters;

namespace PostaFlya.Areas.MobileApi.Infrastructure.HttpFilters
{

    public class MobileApiValidationActionFilter : ApiValidationActionFilterBaseAttribute
    {
        protected override object CreateFaultMessage(IDictionary<string, ModelState> modelState)
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