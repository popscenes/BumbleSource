using System.Linq;
using PostaFlya.Domain.Flier.Query;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Schedule;
using Website.Application.SiteMap;
using Website.Application.Util;
using Website.Infrastructure.Configuration;

namespace PostaFlya.Application.Domain.SiteMap
{
    public class SiteMapXmlGenJobAction : JobActionInterface
    {
        public const string SiteMapFileFormat = "sitemap{0}.xml";


        private readonly BlobStorageInterface _applicationStorage;
        private readonly TempFileStorageInterface _tempFileStorage;
        private readonly FlierSearchServiceInterface _flierSearchService;
        private readonly ConfigurationServiceInterface _configurationService;

        public SiteMapXmlGenJobAction([ApplicationStorage]BlobStorageInterface applicationStorage,
                                      TempFileStorageInterface tempFileStorage, FlierSearchServiceInterface flierSearchService,
                                      ConfigurationServiceInterface configurationService)
        {
            _applicationStorage = applicationStorage;
            _tempFileStorage = tempFileStorage;
            _flierSearchService = flierSearchService;
            _configurationService = configurationService;
        }

        public void Run(JobBase job)
        {
            const int skiptake = 50;

            var site = _configurationService.GetSetting("SiteUrl");
            using (var siteMapIndex = new SiteMapIndexBuilder(site, SiteMapFileFormat, _tempFileStorage, _applicationStorage))
            {          
                var skip = 0;
                var flierIds = _flierSearchService.IterateAllIndexedFliers(skiptake, skip, true);
                while (flierIds.Any())
                {
                    foreach (var flierId in flierIds)
                    {
                        siteMapIndex.AddPath("/" + flierId);
                    }
                    skip += skiptake;
                    flierIds = _flierSearchService.IterateAllIndexedFliers(skiptake, skip, true);
                }    
            }
            
        }
    }
}