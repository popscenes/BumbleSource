using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Ninject;
using PostaFlya.Application.Domain.Browser;

namespace PostaFlya.Attributes
{
    public class BrowserInformationRequiredAttribute : ActionFilterAttribute
    {
        [Inject]
        public BrowserInformationInterface BrowserInformation { get; set; }

        //public override void OnActionExecuting(HttpActionContext actionContext)
        //{
        //    HttpContext
        //}

    }
}
