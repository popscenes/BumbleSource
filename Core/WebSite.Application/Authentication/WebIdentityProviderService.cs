using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Infrastructure.Authentication;
using WebSite.Application.WebsiteInformation;
using System.Web;

namespace WebSite.Application.Authentication
{
    public class WebIdentityProviderService : IdentityProviderServiceInterface
    {
        public IEnumerable<IdentityProviderInterface> Get()
        {
            throw new NotImplementedException();
        }

        private WebsiteInfoServiceInterface _websiteInfoService;
        private HttpContextBase _httpContext;

        public WebIdentityProviderService(WebsiteInfoServiceInterface websiteInfoService, HttpContextBase httpContext)
        {
            _websiteInfoService = websiteInfoService;
            _httpContext = httpContext;
        }

        public IdentityProviderInterface GetProviderByIdentifier(string providerIdentifier)
        {
            
            if(providerIdentifier.Contains(IdentityProviders.GOOGLE))
            {
                return new OpenIdIdentityProvider() {Identifier = providerIdentifier, Name = IdentityProviders.GOOGLE, };
            }

            if (providerIdentifier.Contains(IdentityProviders.FACEBOOK))
            {
                var websiteInfo = _websiteInfoService.GetWebsiteInfo(_httpContext.Request.Url.Host);
                return new HomeRolledFacebookProvider(_httpContext, websiteInfo.FacebookAppID, websiteInfo.FacebookAppSecret)
                           {
                               Identifier = providerIdentifier, Name = IdentityProviders.FACEBOOK
                           };
            }

            return null;
        }
    }
}
