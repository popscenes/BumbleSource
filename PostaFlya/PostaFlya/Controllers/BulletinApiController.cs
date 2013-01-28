using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Application.Domain.Flier;
using PostaFlya.Domain.Flier.Analytic;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Flier.Payment;
using Website.Application.Binding;
using PostaFlya.Areas.Default.Models;
using PostaFlya.Domain.Behaviour.Query;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Models.Content;
using PostaFlya.Models.Factory;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using Website.Application.Content;
using Website.Application.Domain.Content;
using Website.Domain.Tag;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class BulletinApiController : ApiController
    {
        private readonly GenericQueryServiceInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierBehaviourQueryServiceInterface _behaviourQueryService;
        private readonly FlierBehaviourViewModelFactoryInterface _viewModelFactory;
        private readonly FlierSearchServiceInterface _flierSearchService;
        private readonly FlierWebAnalyticServiceInterface _webAnalyticService;
        private readonly TinyUrlServiceInterface _tinyUrlService;
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;


        public BulletinApiController(GenericQueryServiceInterface queryService,
            [ImageStorage]BlobStorageInterface blobStorage
            , FlierBehaviourQueryServiceInterface behaviourQueryService
            , FlierBehaviourViewModelFactoryInterface viewModelFactory, FlierSearchServiceInterface flierSearchService
            , FlierWebAnalyticServiceInterface webAnalyticService, TinyUrlServiceInterface tinyUrlService
            , PostaFlyaBrowserInformationInterface browserInformation)
        {
            _queryService = queryService;
            _blobStorage = blobStorage;
            _behaviourQueryService = behaviourQueryService;
            _viewModelFactory = viewModelFactory;
            _flierSearchService = flierSearchService;
            _webAnalyticService = webAnalyticService;
            _tinyUrlService = tinyUrlService;
            _browserInformation = browserInformation;
        }

        public DefaultDetailsViewModel Get([FromUri] LocationModel currloc, string tinyUrl)
        {
            var ent = _tinyUrlService.EntityInfoFor(tinyUrl);
            if (ent == null)
                return null;

            var ret = GetDetail(ent.Id, _queryService, _behaviourQueryService, _blobStorage, _viewModelFactory, _browserInformation);
            
            if (ret != null)
                _webAnalyticService.RecordVisit(ent.Id, FlierAnalyticSourceAction.TinyUrlByApi, currloc.ToDomainModel());

            return ret;
        }

        public DefaultDetailsViewModel Get(string id)
        {
            var ret = GetDetail(id, _queryService, _behaviourQueryService, _blobStorage, _viewModelFactory, _browserInformation);
            
            if(ret != null)
                _webAnalyticService.RecordVisit(ret.Flier.Id, FlierAnalyticSourceAction.IdByApi);
            
            return ret;
        }

        public IList<BulletinFlierModel> Get([FromUri]LocationModel loc
            ,int count, string board = "", int skip = 0, int distance = 0, string tags = "")
        {
            _webAnalyticService.SetLastSearchLocation(loc.ToDomainModel());
            return GetFliers(_flierSearchService, _queryService, _blobStorage, _viewModelFactory
                             , loc, count, board, skip, distance, tags);
        }

        [NonAction]
        public static DefaultDetailsViewModel GetDetail(string id, GenericQueryServiceInterface queryService, FlierBehaviourQueryServiceInterface behaviourQueryService, BlobStorageInterface blobStorage, FlierBehaviourViewModelFactoryInterface viewModelFactory, PostaFlyaBrowserInformationInterface browserInformation)
        {
            
            var flier = queryService.FindByFriendlyId<Flier>(id);
            if (flier == null)
                return null;

            var behaviour = behaviourQueryService.GetBehaviourFor(flier);
            if (behaviour == null)
                return null;

            var ret = viewModelFactory
                .GetBehaviourViewModel(behaviour);
            ret.Flier.GetImageUrl(blobStorage);
            ret.Flier.ImageList.ForEach(_ => _.GetImageUrl(blobStorage, ThumbOrientation.Horizontal, ThumbSize.S50));

            if (browserInformation.Browser.Id == flier.BrowserId)
                AddOwnerInfo(flier, ret, queryService);

            return ret;
        }

        public static void AddOwnerInfo(FlierInterface flier, DefaultDetailsViewModel model
            , GenericQueryServiceInterface queryService)
        {
            if (!flier.HasFeatureAndIsEnabled(AnalyticsFeatureChargeBehaviour.Description)) return;
            var list = queryService.FindAggregateEntities<FlierAnalytic>(flier.Id).ToList();
            model.AnalyticInfo = list.ToInfo().ToModel();//if this is inefficient in long run move to qs


        }


        [NonAction]
        public static IList<BulletinFlierModel> GetFliers(FlierSearchServiceInterface flierSearchService
            , GenericQueryServiceInterface flierQueryService
            , BlobStorageInterface blobStorage, FlierBehaviourViewModelFactoryInterface viewModelFactory
            , LocationModel loc, int count, string board = "", int skip = 0, int distance = 0, string tags = "")
        {
            var locDomainModel = loc.ToDomainModel();
            var tagsModel = new Tags(tags);

            //            location.Latitude = -37.7654897;
            //            location.Longitude = 144.9770748;
            distance = Math.Min(distance, 30);
            count = Math.Min(count, 50);

            var fliersIds =
                flierSearchService.FindFliersByLocationTagsAndDistance(locDomainModel,
                tagsModel, board, distance, count, FlierSortOrder.CreatedDate, skip);

            var watch = new Stopwatch();
            watch.Start();
            var ret = fliersIds
                .Select(f => viewModelFactory.GetBulletinViewModel(flierQueryService.FindById<Flier>(f), false)
                    .GetImageUrl(blobStorage).GetContactDetails(flierQueryService))
                .ToList();
            Trace.TraceInformation("Bulletin Get FindById time: {0}, numfliers {1}", watch.ElapsedMilliseconds, ret.Count());
            return ret;
        }


    }
}