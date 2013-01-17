using System.Web;
using Website.Application.Domain.Browser;
using Website.Domain.Location;
using Website.Infrastructure.Query;

namespace PostaFlya.Application.Domain.Browser
{
    public interface PostaFlyaBrowserInformationInterface : BrowserInformationInterface
    {
        Location LastLocation { get; set; }
        bool LocationFromDevice { get; set; }
    }

    public class PostaFlyaBrowserInformation : BrowserInformation, PostaFlyaBrowserInformationInterface
    {
        private readonly HttpContextBase _httpContext;

        public PostaFlyaBrowserInformation(GenericQueryServiceInterface browserQueryService, HttpContextBase httpContext) 
            : base(browserQueryService, httpContext)
        {
            _httpContext = httpContext;
        }

        public Location LastLocation
        {
            get { return GetClientStateValue<Location>("LastLocation"); }
            set { SetClientStateValue("LastLocation", value); }
        }

        public bool LocationFromDevice
        {
            get { return GetClientStateValue<bool>("LocationFromDevice"); }
            set { SetClientStateValue("LocationFromDevice", value); }
        }
    }
}