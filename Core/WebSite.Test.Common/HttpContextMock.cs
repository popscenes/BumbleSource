using System.IO;
using System.Security.Principal;
using System.Web;
using Moq;
using Ninject.MockingKernel.Moq;

namespace WebSite.Test.Common
{
    public static class HttpContextMock
    {
        public static void HttpContextCurrentCreate(string requestUrl, string requestQueryString)
        {
            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://localhost:1805/account/Authenticate", requestQueryString),
                new HttpResponse(new StringWriter())
                )
                                      {
                                          User = new GenericPrincipal(
                                              new GenericIdentity("username"),
                                              new string[0]
                                              )
                                      };

            // User is logged in 

            HttpContext.Current.Request.RequestType = "GET";
        }

        public static void FakeHttpContext(MoqMockingKernel kernel)
        {
            kernel.Unbind<HttpContextBase>();
            kernel.Unbind<HttpResponseBase>();
            kernel.Unbind<HttpRequestBase>();

            var context = kernel.GetMock<HttpContextBase>();
            var request = kernel.GetMock<HttpRequestBase>();
            var response = kernel.GetMock<HttpResponseBase>();
            var session = kernel.GetMock<HttpSessionStateBase>();
            var server = kernel.GetMock<HttpServerUtilityBase>();

            request.Setup(req => req.Cookies).Returns(new HttpCookieCollection());
            response.Setup(req => req.Cookies).Returns(new HttpCookieCollection());
            response.SetupProperty(res => res.StatusCode);
            response.SetupProperty(res => res.RedirectLocation);
            response.SetupGet(res => res.IsRequestBeingRedirected);
            response.Setup(res => res.Redirect(It.IsAny<string>())).Callback((string s) => response.Object.RedirectLocation = s);


            context.Setup(ctx => ctx.Request).Returns(request.Object);
            context.Setup(ctx => ctx.Response).Returns(response.Object);
            context.Setup(ctx => ctx.Session).Returns(session.Object);
            context.Setup(ctx => ctx.Server).Returns(server.Object);
           


            kernel.Bind<HttpContextBase>()
                .ToConstant(context.Object).InSingletonScope();

            kernel.Bind<HttpResponseBase>()
                .ToConstant(response.Object).InSingletonScope();

            kernel.Bind<HttpRequestBase>()
               .ToConstant(request.Object).InSingletonScope();
        }

    }
}
