using System;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Web;
using Newtonsoft.Json;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Authentication;
using Website.Domain.Browser;
using Website.Infrastructure.Query;

namespace Website.Application.Domain.Browser
{
    public class BrowserInformation : BrowserInformationInterface
    {
        private readonly HttpContextBase _httpContext;

        public const string BrowserCookieId = "browserInformation";
        public const string TempBrowserId = "tempId";

        public BrowserInformation(GenericQueryServiceInterface browserQueryService, HttpContextBase httpContext)
        {
            _httpContext = httpContext;

            IpAddress = _httpContext.Request.UserHostAddress;
            UserAgent = _httpContext.Request.UserAgent;
            
            var identity = (WebIdentityInterface)_httpContext.User.Identity;
            BrowserInterface browser = null;
            if (identity.IsAuthenticated 
                && (browser = browserQueryService.FindBrowserByIdentityProvider(identity.ToCredential())) != null)
            {
                //reset the roles
                _httpContext.User = new GenericPrincipal(HttpContext.Current.User.Identity, browser.Roles.ToArray());
            }
            if (browser == null)
                browser = AnonymousBrowserGet();

            Browser = browser;
        }

        //don't over use these mmmK?
        protected ValType GetClientStateValue<ValType>(string property)
        {
            var browserCookie = _httpContext.Request.Cookies[BrowserCookieId];
            if (browserCookie == null)
                return default(ValType);

            var val = browserCookie.Values[property];
            if(val == null)
                return default(ValType);

            try
            {
                return JsonConvert.DeserializeObject<ValType>(val);
            }
            catch (Exception e)
            {
                Trace.TraceWarning("failed to deserialize client state value {0} value = {1}", property, val);
            }

            return default(ValType);
        }
        protected void SetClientStateValue<ValType>(string property, ValType value)
        {
            var browserCookie = _httpContext.Request.Cookies[BrowserCookieId] ??
                                       new HttpCookie(BrowserCookieId);

            browserCookie.Values[property] = JsonConvert.SerializeObject(value);

            browserCookie.Expires = DateTime.Now.AddYears(1);
            _httpContext.Response.Cookies.Add(browserCookie);
        }

        public BrowserInterface AnonymousBrowserGet()
        {
            if (Browser != null)
                return Browser;

            var browserId = GetClientStateValue<string>(TempBrowserId);
            if (browserId == null)
            {
                browserId = Guid.NewGuid().ToString();
                SetClientStateValue(TempBrowserId, browserId);
            }

            return new Website.Domain.Browser.Browser(browserId)
                {
                    FriendlyId = "Anonymous-Browser",
                    Roles = new Website.Domain.Browser.Roles { Role.Temporary.ToString() },
                };
        }

        #region Implementation of BrowserInformationInterface

        public BrowserInterface Browser { get; set; }
        public string IpAddress { get; private set; }
        public string UserAgent { get; private set; }

        public string TrackingId
        {
            get { return GetClientStateValue<string>("t"); }
            set { SetClientStateValue("t", value); }
        }

        #endregion
    }
}