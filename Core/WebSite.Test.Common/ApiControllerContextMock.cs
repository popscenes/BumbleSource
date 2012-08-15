using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Hosting;
using System.Web.Mvc;
using Ninject.MockingKernel.Moq;

namespace WebSite.Test.Common
{
    public static class ApiControllerContextMock
    {
        public static void FakeHttpControllerContext(MoqMockingKernel kernel, ApiController controller, string requestUri = "http://localhost/test/")
        {
            var request = new HttpRequestMessage { RequestUri = new Uri(requestUri) };
            var contContext = new Moq.Mock<HttpControllerContext>();
            controller.Request = request;

            controller.ControllerContext = contContext.Object;

            controller.ControllerContext.Request = request;
            controller.ControllerContext.Controller = controller;
            controller.ControllerContext.Configuration = new HttpConfiguration();
            
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = controller.ControllerContext.Configuration;
            
        }
    }
}
