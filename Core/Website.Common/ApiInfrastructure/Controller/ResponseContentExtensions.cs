using System.Net;
using System.Net.Http;
using System.Web.Http;
using Website.Common.ApiInfrastructure.Model;

namespace Website.Common.ApiInfrastructure.Controller
{
    public static class ResponseContentExtensions
    {
        public static HttpResponseMessage Response<T>(this ApiController controller, HttpStatusCode status, ResponseContent<T> message)
        {
            var res = controller.ControllerContext.Request.CreateResponse(status, message);
            return res;
        }

        public static void ResponseError(this ApiController controller, HttpStatusCode status, ResponseContent message)
        {
            var res = controller.ControllerContext.Request.CreateResponse(status, message);
            throw new HttpResponseException(res);
        }

        public static ResponseContent ResponseError(this ApiController controller, HttpStatusCode status, ResponseContent.StatusEnum contentStatus, string messageFormat = "", params object[] messageParams)
        {
            var message = new ResponseContent(contentStatus, messageFormat, messageParams);
            controller.ResponseError(status, message);
            return message;
        }

        public static ResponseContent ResponseNotFound(this ApiController controller, string messageFormat = "", params object[] messageParams)
        {
            return controller.ResponseError(HttpStatusCode.NotFound, ResponseContent.StatusEnum.NotFound, messageFormat, messageParams);
        }

        public static ResponseContent ResponseUnauthorized(this ApiController controller, string messageFormat = "", params object[] messageParams)
        {
            return controller.ResponseError(HttpStatusCode.Unauthorized, ResponseContent.StatusEnum.Unauthorized, messageFormat, messageParams);
        }
    }
}