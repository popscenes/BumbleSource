using System;
using System.Web;
using Website.Application.Domain.Browser;
using Website.Domain.Browser;
using Website.Domain.Location;
using Website.Infrastructure.Query;

namespace PostaFlya.Application.Domain.Browser
{
    public interface PostaFlyaBrowserInformationInterface : BrowserInformationInterface
    {
        Location LastSearchLocation { get; set; }
        PostaFlya.Domain.Browser.Browser PostaBrowser { get; }
    }

    public class PostaFlyaBrowserInformation : BrowserInformation<PostaFlya.Domain.Browser.Browser>, PostaFlyaBrowserInformationInterface
    {
        private readonly GenericQueryServiceInterface _genericQueryService;
        public PostaFlyaBrowserInformation(GenericQueryServiceInterface genericQueryService, HttpContextBase httpContext, QueryChannelInterface queryChannel)
            : base(genericQueryService, httpContext, queryChannel)
        {
            _genericQueryService = genericQueryService;
        }


        public Location LastSearchLocation
        {
            get { return GetClientStateValue<Location>("LastSearchLocation"); }
            set { SetClientStateValue("LastSearchLocation", value); }
        }

        public PostaFlya.Domain.Browser.Browser PostaBrowser {
            get { return Browser as PostaFlya.Domain.Browser.Browser; }
        }

        public override PostaFlya.Domain.Browser.Browser AnonymousBrowserGet()
        {
            var browserId = GetClientStateValue<string>(TempBrowserId);
            if (browserId == null)
            {
                browserId = Guid.NewGuid().ToString();
                SetClientStateValue(TempBrowserId, browserId);
            }

            return new PostaFlya.Domain.Browser.Browser()
            {
                Id = browserId,
                FriendlyId = "Anonymous-Browser",
                Roles = new Website.Domain.Browser.Roles { Role.Temporary.ToString() },
            };
        }

        public override PostaFlya.Domain.Browser.Browser BrowserGetById(string browserId)
        {
            return _genericQueryService.FindById<PostaFlya.Domain.Browser.Browser>(browserId);
        }
    }
}