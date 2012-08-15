using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Application.Caching.Query;
using System.Runtime.Caching;
using WebSite.Infrastructure.Binding;

namespace WebSite.Application.WebsiteInformation
{
    public class CachedWebsiteInfoService : TimedExpiryCachedQueryService
                                           , WebsiteInfoServiceInterface
    {
        private readonly WebsiteInfoServiceInterface _queryService;
        
        public CachedWebsiteInfoService([SourceDataSource]WebsiteInfoServiceInterface queryService
                , ObjectCache cacheProvider
                , int defaultSecondsToCache = -1)
            : base(cacheProvider, "websiteinfo", defaultSecondsToCache)
        {
            _queryService = queryService;
        }


        public void RegisterWebsite(string url, WebsiteInfo GetWebsiteInfo)
        {
            _queryService.RegisterWebsite(url, GetWebsiteInfo);
        }

        public string GetBehaivourTags(string url)
        {
            return RetrieveCachedData(
                GetKeyFor("behaivourtags", url),
                () => this._queryService.GetBehaivourTags(url));
        }

        public string GetTags(string url)
        {
            return RetrieveCachedData(
                GetKeyFor("tags", url),
                () => this._queryService.GetTags(url));
        }

        public string GetWebsiteName(string url)
        {
            return RetrieveCachedData(
                GetKeyFor("websitename", url),
                () => this._queryService.GetWebsiteName(url));
        }

        public WebsiteInfo GetWebsiteInfo(string url)
        {
            return RetrieveCachedData(
                GetKeyFor("websiteInfo", url),
                () => this._queryService.GetWebsiteInfo(url));
        }
    }
}
