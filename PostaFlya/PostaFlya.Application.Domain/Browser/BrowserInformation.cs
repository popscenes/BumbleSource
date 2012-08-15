using System;
using System.Security.Principal;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Query;
using WebSite.Infrastructure.Authentication;
using System.Web;

namespace PostaFlya.Application.Domain.Browser
{
    public class BrowserInformation : BrowserInformationInterface
    {
        private readonly BrowserQueryServiceInterface _browserQueryService;
        private readonly HttpContextBase _httpContext;

        protected const string TEMP_BROWSER_COOKIEID = "tempId";

        public BrowserInformation(BrowserQueryServiceInterface browserQueryService, HttpContextBase httpContext)
        {          
            _browserQueryService = browserQueryService;
            _httpContext = httpContext;
            
            var identity = (WebIdentityInterface)_httpContext.User.Identity;
            Browser = !identity.IsAuthenticated ? AnonymousBrowserGet() : browserQueryService.FindByIdentityProvider(identity.ToCredential());
        }

        public BrowserInterface AnonymousBrowserGet()
        {
            if (Browser != null)
                return Browser;

            HttpCookie tempIDCookie = _httpContext.Request.Cookies[TEMP_BROWSER_COOKIEID];

            if (tempIDCookie == null)
            {
                tempIDCookie = new HttpCookie(TEMP_BROWSER_COOKIEID, Guid.NewGuid().ToString());
                _httpContext.Response.Cookies.Add(tempIDCookie);

            }

            return new PostaFlya.Domain.Browser.Browser(tempIDCookie.Value);
        }

        #region Implementation of BrowserInformationInterface

        public BrowserInterface Browser { get; set; }
        #endregion
    }
}