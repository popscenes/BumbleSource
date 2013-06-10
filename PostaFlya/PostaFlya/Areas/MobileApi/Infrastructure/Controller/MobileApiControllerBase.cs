using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using PostaFlya.Areas.MobileApi.Infrastructure.HttpFilters;

namespace PostaFlya.Areas.MobileApi.Infrastructure.Controller
{
    [MobileApiValidationActionFilter]
    public class MobileApiControllerBase : ApiController
    {
    }
}