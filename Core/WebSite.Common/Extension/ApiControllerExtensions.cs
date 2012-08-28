using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Command;

namespace Website.Common.Extension
{
    public static class ApiControllerExtensions
    {
        public static Uri GetEntityUri(this ApiController controller, string entityid)
        {
//            var controllerArg = controller.ControllerContext.RouteData.Values["Controller"];
//            var flierUrl = controller.Url.Route("DefaultApi", new { Controller = controllerArg, id = entityid });
            return controller.ControllerContext == null ? new Uri("http://localhost/test/" + entityid) 
                : new Uri(controller.ControllerContext.Request.RequestUri + "/" +entityid);
        }

        public static HttpResponseMessage GetResponseForRes(this ApiController controller, object res)
        {
            var msg = (res is MsgResponse) ? (MsgResponse) res : null;
            HttpResponseMessage responseMessage;

            if ((msg != null && !msg.IsError))
            {
                responseMessage = controller.ControllerContext.Request.CreateResponse(HttpStatusCode.Created, msg);
                var entityId = msg.GetEntityId();
                if (entityId != null)
                    responseMessage.Headers.Location = controller.GetEntityUri(entityId);
            }
            else
            {
                if(msg == null)
                    msg = new MsgResponse("Entity Create/Modify failed", true);
                responseMessage = controller.ControllerContext.Request.CreateResponse(HttpStatusCode.BadRequest, msg);
            }
            return responseMessage; 
        }

    }
}