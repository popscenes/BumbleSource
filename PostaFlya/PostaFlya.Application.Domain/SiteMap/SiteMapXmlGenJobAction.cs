using System.Linq;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Schedule;
using Website.Application.SiteMap;
using Website.Application.Util;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Query;

namespace PostaFlya.Application.Domain.SiteMap
{
    public class SiteMapXmlGenJobAction : JobActionInterface
    {
        public const string SiteMapFileFormat = "sitemap{0}.xml";


        private readonly BlobStorageInterface _applicationStorage;
        private readonly TempFileStorageInterface _tempFileStorage;
        private readonly FlierSearchServiceInterface _flierSearchService;
        private readonly ConfigurationServiceInterface _configurationService;
        private readonly GenericQueryServiceInterface _queryService;

        public SiteMapXmlGenJobAction([ApplicationStorage]BlobStorageInterface applicationStorage,
                                      TempFileStorageInterface tempFileStorage, FlierSearchServiceInterface flierSearchService,
                                      ConfigurationServiceInterface configurationService, GenericQueryServiceInterface queryService)
        {
            _applicationStorage = applicationStorage;
            _tempFileStorage = tempFileStorage;
            _flierSearchService = flierSearchService;
            _configurationService = configurationService;
            _queryService = queryService;
        }

        public void Run(JobBase job)
        {
            const int skiptake = 1000;

            var site = _configurationService.GetSetting("SiteUrl");
            using (var siteMapIndex = new SiteMapIndexBuilder(site, SiteMapFileFormat, _tempFileStorage, _applicationStorage))
            {
                var flierIds = _flierSearchService.IterateAllIndexedFliers(skiptake, null, returnFriendlyId: true);
                while (flierIds.Any())
                {
                    foreach (var flierId in flierIds)
                    {
                        siteMapIndex.AddPath("/" + flierId);
                    }
                    var skip = _queryService.FindByFriendlyId<PostaFlya.Domain.Flier.Flier>(flierIds.Last()); 
                    flierIds = _flierSearchService.IterateAllIndexedFliers(skiptake, skip, returnFriendlyId: true);
                }    
            }
            
        }
    }
}