using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Caching;
using Website.Application.Caching.Query;
using Website.Infrastructure.Binding;

namespace Website.Application.WebsiteInformation
{
    public class CachedWebsiteInfoService : TimedExpiryCachedQueryService
                                           , WebsiteInfoServiceInterface
    {
        private readonly WebsiteInfoServiceInterface _queryService;
        
        public CachedWebsiteInfoService([SourceDataSource]WebsiteInfoServiceInterface queryService
                , ObjectCache cacheProvider
                , int defaultSecondsToCache = -1)
            : base(cacheProvider, null, defaultSecondsToCache)
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
                "behaivourtags" + url,
                () => this._queryService.GetBehaivourTags(url));
        }

        public string GetTags(string url)
        {
            return RetrieveCachedData(
                "tags" + url,
                () => this._queryService.GetTags(url));
        }

        public string GetWebsiteName(string url)
        {
            return RetrieveCachedData(
                "websitename" + url,
                () => this._queryService.GetWebsiteName(url));
        }

        public WebsiteInfo GetWebsiteInfo(string url)
        {
            return RetrieveCachedData(
                "websiteInfo" + url,
                () => this._queryService.GetWebsiteInfo(url));
        }
    }
}
