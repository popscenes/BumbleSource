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
    public class DefaultBrowserInformation : BrowserInformation<Website.Domain.Browser.Browser>
    {
        private readonly GenericQueryServiceInterface _browserQueryService;

        public DefaultBrowserInformation(GenericQueryServiceInterface browserQueryService
            , HttpContextBase httpContext, QueryChannelInterface queryChannel)
            : base(browserQueryService, httpContext, queryChannel)
        {
            _browserQueryService = browserQueryService;
        }

        public override Website.Domain.Browser.Browser AnonymousBrowserGet()
        {
            var browserId = GetClientStateValue<string>(TempBrowserId);
            if (browserId == null)
            {
                browserId = Guid.NewGuid().ToString();
                SetClientStateValue(TempBrowserId, browserId);
            }

            return new Website.Domain.Browser.Browser()
            {
                Id = browserId,
                FriendlyId = "Anonymous-Browser",
                Roles = new Website.Domain.Browser.Roles { Role.Temporary.ToString() },
            };
        }

        public override Website.Domain.Browser.Browser BrowserGetById(string browserId)
        {
            return _browserQueryService.FindById<Website.Domain.Browser.Browser>(browserId);
        }
    }

    public abstract class BrowserInformation<BrowserType> : BrowserInformationInterface where BrowserType : class, BrowserInterface
    {
        private readonly GenericQueryServiceInterface _browserQueryService;
        private readonly QueryChannelInterface _queryChannel;
        private readonly HttpContextBase _httpContext;

        public const string BrowserCookieId = "browserInformation";
        public const string TempBrowserId = "tempId";

        protected BrowserInformation(GenericQueryServiceInterface browserQueryService, HttpContextBase httpContext, QueryChannelInterface queryChannel)
        {
            _browserQueryService = browserQueryService;
            _httpContext = httpContext;
            _queryChannel = queryChannel;

            IpAddress = _httpContext.Request.UserHostAddress;
            UserAgent = _httpContext.Request.UserAgent;
            
        }

        private void PopulateBrowser()
        {
            if (_browser != null)
                return;
            var identity = (WebIdentityInterface)_httpContext.User.Identity;
            BrowserType browser = null;
            if (identity.IsAuthenticated)
            {
                browser = _queryChannel.Query(new FindBrowserByIdentityProviderQuery() { Credential = identity.ToCredential() }, (BrowserType)null);

                if (browser != null)
                    _httpContext.User = new GenericPrincipal(_httpContext.User.Identity, browser.Roles.ToArray());
            }
            if (browser == null)
                browser = AnonymousBrowserGet();

            _browser = browser;
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

        public abstract BrowserType AnonymousBrowserGet();
        public abstract BrowserType BrowserGetById(string browserId);

        #region Implementation of BrowserInformationInterface

        private BrowserType _browser;
        public BrowserInterface Browser
        {
            get
            {
                PopulateBrowser();
                return _browser;
            }
        }

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