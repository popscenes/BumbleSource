using System.Web;
using System.Web.Mvc;
using Ninject.MockingKernel.Moq;

namespace WebSite.Test.Common
{
    public static class ControllerContextMock
    {
        public static void FakeControllerContext(MoqMockingKernel kernel, Controller controller)
        {
            
            var httpContext = kernel.GetMock<HttpContextBase>();

            var contContext = new Moq.Mock<ControllerContext>();
            contContext.Setup(c => c.HttpContext)
                .Returns(httpContext.Object);
            
            controller.ControllerContext = contContext.Object;
        }

    }
}
