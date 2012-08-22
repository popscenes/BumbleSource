using System.Runtime.Caching;
using WebSite.Application.Binding;
using WebSite.Application.Caching.Query;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Query;
using WebSite.Infrastructure.Authentication;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Query;
using WebSite.Infrastructure.Binding;

namespace PostaFlya.Application.Domain.Browser.Query
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

        public BrowserInterface FindByHandle(string handle)
        {
            return RetrieveCachedData(
                GetKeyFor(CachedBrowserContext.Browser, handle),
                () => _browserQueryService.FindByHandle(handle));
        }

    }
}