using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Schedule;
using Website.Application.SiteMap;
using Website.Application.Util;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Application.Domain.SiteMap
{
    public class SiteMapXmlGenJobAction : JobActionInterface
    {
        public const string SiteMapFileFormat = "sitemap{0}.xml";


        private readonly BlobStorageInterface _applicationStorage;
        private readonly TempFileStorageInterface _tempFileStorage;
        //private readonly FlierSearchServiceInterface _flierSearchService;
        private readonly ConfigurationServiceInterface _configurationService;
        private readonly GenericQueryServiceInterface _queryService;

        public SiteMapXmlGenJobAction([ApplicationStorage]BlobStorageInterface applicationStorage,
                                      TempFileStorageInterface tempFileStorage//, FlierSearchServiceInterface flierSearchService
                                        , ConfigurationServiceInterface configurationService, GenericQueryServiceInterface queryService)
        {
            _applicationStorage = applicationStorage;
            _tempFileStorage = tempFileStorage;
            //_flierSearchService = flierSearchService;
            _configurationService = configurationService;
            _queryService = queryService;
        }

        public void Run(JobBase job)
        {
            const int skiptake = 1000;

            var site = _configurationService.GetSetting("SiteUrl");
            using (var siteMapIndex = new SiteMapIndexBuilder(site, SiteMapFileFormat, _tempFileStorage, _applicationStorage))
            {
                IQueryable<string> flierIds = new List<string>().AsQueryable();
                do
                {
                    var skip = flierIds.LastOrDefault();

                    flierIds = _queryService.GetAllIds<PostaFlya.Domain.Flier.Flier>(skip, skiptake);
                    foreach (var flierId in flierIds.Select(id => _queryService.FindById<PostaFlya.Domain.Flier.Flier>(id)))
                    {
                        siteMapIndex.AddPath("/" + flierId.FriendlyId);
                    }
                } while (flierIds.Any());
            }
        }
    }
}