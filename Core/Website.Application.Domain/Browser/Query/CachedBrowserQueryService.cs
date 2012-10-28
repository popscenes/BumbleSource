using System.Runtime.Caching;
using Website.Application.Caching.Query;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Binding;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;

namespace Website.Application.Domain.Browser.Query
{
    public class CachedBrowserQueryService : TimedExpiryCachedQueryService,
                                             BrowserQueryServiceInterface
    {
        private readonly BrowserQueryServiceInterface _browserQueryService;
        public CachedBrowserQueryService([SourceDataSource]BrowserQueryServiceInterface browserQueryService
                        , ObjectCache cacheProvider                      
                        , int defaultSecondsToCache = -1)
            : base(cacheProvider, CachedBrowserContext.Region, browserQueryService, defaultSecondsToCache)
        {
            _browserQueryService = browserQueryService;
        }

        public BrowserInterface FindByIdentityProvider(IdentityProviderCredential credential)
        {
            return RetrieveCachedData(
                GetKeyFor(CachedBrowserContext.Identity, credential.GetHash()),
                () => _browserQueryService.FindByIdentityProvider(credential));
        }

    }
}