using System;
using System.Web;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Authentication;
using Website.Domain.Browser;
using Website.Infrastructure.Query;

namespace Website.Application.Domain.Browser
{
    public class BrowserInformation : BrowserInformationInterface
    {
        private readonly HttpContextBase _httpContext;

        protected const string TEMP_BROWSER_COOKIEID = "tempId";

        public BrowserInformation(GenericQueryServiceInterface browserQueryService, HttpContextBase httpContext)
        {
            _httpContext = httpContext;
            
            var identity = (WebIdentityInterface)_httpContext.User.Identity;
            Browser = (!identity.IsAuthenticated ? AnonymousBrowserGet() : browserQueryService.FindBrowserByIdentityProvider(identity.ToCredential())) ??
                      AnonymousBrowserGet();
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

            return new Website.Domain.Browser.Browser(tempIDCookie.Value);
        }

        #region Implementation of BrowserInformationInterface

        public BrowserInterface Browser { get; set; }
        #endregion
    }
}