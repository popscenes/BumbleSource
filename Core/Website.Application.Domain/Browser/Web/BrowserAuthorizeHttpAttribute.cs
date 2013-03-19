using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Ninject;
using Ninject.Syntax;
using Website.Domain.Browser;
using Website.Infrastructure.Command;
using Website.Infrastructure.Util;

namespace Website.Application.Domain.Browser.Web
{
    public class BrowserAuthorizeHttpAttribute : ActionFilterAttribute
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
                _rolesSplit = value.SplitStringTrimRemoveEmpty();
            }
        }

        [Inject]
        public IResolutionRoot ResolutionRoot { get; set; }

        protected BrowserInformationInterface BrowserInformation {
            get { return ResolutionRoot.Get<BrowserInformationInterface>(); }}

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var browserid = TryGetIdFromRequest(actionContext);
            var handle = TryGetHandleFromRequest(actionContext);

            if (BrowserInformation.Browser.HasRole(Role.Admin))
                return;
            if (IsValidBrowserIdOrHandle(browserid, handle))
                return;
              
//
//#if DEBUG
//            return;
//#endif
           
            var faultMessage  = new MsgResponse {Message = "Invalid Access", IsError = true};
            var responseMessage = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, faultMessage);
            actionContext.Response = responseMessage;
        }

        private bool IsValidBrowserIdOrHandle(string browserid, string handle)
        {
            return (
                       BrowserInformation.IsOrCanAccessBrowser(browserid, handle)
                        &&
                        (
                            (Roles.Length == 0 && !BrowserInformation.Browser.IsTemporary()) ||
                            BrowserInformation.Browser.HasAnyRole(_rolesSplit)
                        )
                   );
        }

        private static string TryGetHandleFromRequest(HttpActionContext actionContext)
        {
            var handle = actionContext.ControllerContext.RouteData.Values["handle"] as string;
            if (string.IsNullOrWhiteSpace(handle))
            {
                foreach (var browserInt in actionContext.ActionArguments.Select(arg => arg.Value as BrowserInterface))
                {
                    object browOut;
                    if (browserInt != null)
                    {
                        handle = browserInt.FriendlyId;
                        break;
                    }
                    else if (actionContext.ActionArguments.TryGetValue("handle", out browOut))
                    {
                        handle = browOut as string;
                        if (!string.IsNullOrWhiteSpace(handle))
                            break;
                    }
                }
            }
            return handle;
        }

        private static string TryGetIdFromRequest(HttpActionContext actionContext)
        {
            var browserid = actionContext.ControllerContext.RouteData.Values["BrowserId"] as string;
            if (string.IsNullOrWhiteSpace(browserid))
            {
                foreach (var browserInt in actionContext.ActionArguments.Select(arg => arg.Value as BrowserIdInterface))
                {
                    object browOut;
                    if (browserInt != null)
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
            return browserid;
        }
    }
}