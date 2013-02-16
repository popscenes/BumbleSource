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
            //return; if you want to generate fliers without worrying bout authorization

            var browserid = actionContext.ControllerContext.RouteData.Values["BrowserId"] as string;
            if (string.IsNullOrWhiteSpace(browserid))
            {
                foreach (var browserInt in actionContext.ActionArguments.Select(arg => arg.Value as BrowserIdInterface))
                {
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

            if (BrowserInformation.Browser.HasRole(Role.Admin))
                return;

            if (BrowserInformation.Browser.Id == browserid
                && (
                    (Roles.Length == 0 && !BrowserInformation.Browser.IsTemporary()) || 
                    BrowserInformation.Browser.HasAnyRole(_rolesSplit))
                )
                return;
//
//#if DEBUG
//            return;
//#endif
           
            var faultMessage  = new MsgResponse {Message = "Invalid Access", IsError = true};
            var responseMessage = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized, faultMessage);
            actionContext.Response = responseMessage;
        }

    }
}