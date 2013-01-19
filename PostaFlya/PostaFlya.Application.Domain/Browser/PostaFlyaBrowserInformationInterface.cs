using System.Web;
using Website.Application.Domain.Browser;
using Website.Domain.Location;
using Website.Infrastructure.Query;

namespace PostaFlya.Application.Domain.Browser
{
    public interface PostaFlyaBrowserInformationInterface : BrowserInformationInterface
    {
        Location LastSearchLocation { get; set; }
    }

    public class PostaFlyaBrowserInformation : BrowserInformation, PostaFlyaBrowserInformationInterface
    {
        private readonly HttpContextBase _httpContext;

        public PostaFlyaBrowserInformation(GenericQueryServiceInterface browserQueryService, HttpContextBase httpContext) 
            : base(browserQueryService, httpContext)
        {
            _httpContext = httpContext;
        }

        public Location LastSearchLocation
        {
            get { return GetClientStateValue<Location>("LastSearchLocation"); }
            set { SetClientStateValue("LastSearchLocation", value); }
        }
    }
}