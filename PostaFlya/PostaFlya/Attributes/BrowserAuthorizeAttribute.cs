using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Ninject;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Domain.Browser;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Attributes
{
    public class BrowserAuthorizeAttribute : ActionFilterAttribute
    {
        private string _roles;
        private string[] _rolesSplit = new string[0];

        public string Roles
        {
            get
            {
                return _roles ?? String.Empty;
            }
            set
            {
                _roles = value;
                _rolesSplit = SplitString(value);
            }
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            return;
            var browserInfo = MvcApplication.DependencyResolver.Get<BrowserInformationInterface>();
            //for now just check that the logged in browser is the same
            //in the future if we support oauth and applications etc
            //we can enhance this attribute.
            var browserid = actionContext.ControllerContext.RouteData.Values["BrowserId"] as string;
            if (string.IsNullOrWhiteSpace(browserid))
            {
                foreach (var arg in actionContext.ActionArguments)
                {
                    var browserInt = arg.Value as BrowserIdInterface;
                    object browOut;
                    if(browserInt != null)
                    {
                        browserid = browserInt.BrowserId;
                        break;
                    }
                    else if (actionContext.ActionArguments.TryGetValue("BrowserId", out browOut))
                    {
                        browserid = browOut as string;
                        if (!string.IsNullOrWhiteSpace(browserid))
                            break;                    
                    }
                        
               }
                
            }

            if (browserInfo.Browser.Id == browserid && (Roles.Length == 0 ||
                browserInfo.Browser.HasAnyRole(_rolesSplit)))
                return;
//
//#if DEBUG
//            return;
//#endif
           
            var faultMessage  = new MsgResponse {Message = "Invalid Access", IsError = true};
            var responseMessage = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, faultMessage);
            actionContext.Response = responseMessage;
        }

        internal static string[] SplitString(string original)
        {
            if (String.IsNullOrEmpty(original))
            {
                return new string[0];
            }

            var split = from piece in original.Split(',')
                        let trimmed = piece.Trim()
                        where !String.IsNullOrEmpty(trimmed)
                        select trimmed;
            return split.ToArray();
        }
    }
}