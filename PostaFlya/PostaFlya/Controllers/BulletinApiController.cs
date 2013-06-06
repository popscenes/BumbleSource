using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Application.Domain.Flier;
using PostaFlya.Domain.Boards;
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
using Website.Common.Controller;
using Website.Domain.Browser;
using Website.Domain.Claims;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class BulletinApiController : WebApiControllerBase
    {
        private readonly GenericQueryServiceInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierBehaviourQueryServiceInterface _behaviourQueryService;
        private readonly FlierBehaviourViewModelFactoryInterface _viewModelFactory;
        private readonly FlierSearchServiceInterface _flierSearchService;
        private readonly FlierWebAnalyticServiceInterface _webAnalyticService;
        private readonly TinyUrlServiceInterface _tinyUrlService;
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;
        private readonly ConfigurationServiceInterface _configurationService;


        public BulletinApiController(GenericQueryServiceInterface queryService,
            [ImageStorage]BlobStorageInterface blobStorage
            , FlierBehaviourQueryServiceInterface behaviourQueryService
            , FlierBehaviourViewModelFactoryInterface viewModelFactory, FlierSearchServiceInterface flierSearchService
            , FlierWebAnalyticServiceInterface webAnalyticService, TinyUrlServiceInterface tinyUrlService
            , PostaFlyaBrowserInformationInterface browserInformation, ConfigurationServiceInterface configurationService)
        {
            _queryService = queryService;
            _blobStorage = blobStorage;
            _behaviourQueryService = behaviourQueryService;
            _viewModelFactory = viewModelFactory;
            _flierSearchService = flierSearchService;
            _webAnalyticService = webAnalyticService;
            _tinyUrlService = tinyUrlService;
            _browserInformation = browserInformation;
            _configurationService = configurationService;
        }

//        public DefaultDetailsViewModel Get([FromUri] LocationModel currloc, string tinyUrl)
//        {
//            
//            var ent = _tinyUrlService.EntityInfoFor(tinyUrl);
//            if (ent == null)
//                return null;
//
//            var ret = GetDetail(ent.Id, _queryService, _behaviourQueryService, _blobStorage, _viewModelFactory, _browserInformation);
//            
//            if (ret != null)
//            {
//                var source = FlierAnalyticUrlUtil.GetSourceAction(tinyUrl, FlierAnalyticSourceAction.Unknown);
//                if(source != FlierAnalyticSourceAction.Unknown)
//                    _webAnalyticService.RecordVisit(ret.Flier.Id, source, currloc.ToDomainModel());
//
//                _webAnalyticService.RecordVisit(ret.Flier.Id, FlierAnalyticSourceAction.TinyUrlByApi, currloc.ToDomainModel());
//
//            }
//
//            return ret;
//        }

        public DefaultDetailsViewModel Get(string id)
        {
            var ret = GetDetail(id, _queryService, _behaviourQueryService, _blobStorage, _viewModelFactory, _browserInformation);
            
            if(ret != null)
                _webAnalyticService.RecordVisit(ret.Flier.Id, FlierAnalyticSourceAction.IdByApi);
            
            return ret;
        }

        /// <summary>
        /// dgdgsdgdsfgg
        /// </summary>
        /// <param name="req">ytrydyr</param>
        /// <returns></returns>
        public IList<BulletinFlierModel> Get([FromUri]BulletinGetRequestModel req)
        {
//            if (!loc.IsValid() && string.IsNullOrWhiteSpace(board))
//                return GetDefaultFliers(count, skipPast, tags);

            if (_browserInformation.LastSearchLocation == null)
            {
                _browserInformation.LastSearchLocation = new Location();
            }
            _browserInformation.LastSearchLocation.CopyFieldsFrom(req.Loc);

            _webAnalyticService.SetLastSearchLocation(req.Loc.ToDomainModel());
            return GetFliers(_flierSearchService, _queryService, _blobStorage, _viewModelFactory
                , req.Loc, req.Count, board: req.Board, skipPast: req.SkipPast, distance: req.Distance, tags: req.Tags, date: req.Date);
        }

//        private IList<BulletinFlierModel> GetDefaultFliers(int count, string skipPast, string tags)
//        {
//            var skipFlier = string.IsNullOrWhiteSpace(skipPast) ? null : _queryService.FindById<Flier>(skipPast);
//            var ids = _flierSearchService.IterateAllIndexedFliers(count, skipFlier, new Tags(tags));
//            if (ids.Count > 0)
//                return BulletinFlierModelUtil.IdsToModel(ids.Select(_ => _.Id), _queryService, _blobStorage, _viewModelFactory);
//            if(skipFlier != null)
//                return new List<BulletinFlierModel>();
//
//            var defaultIds = _configurationService.GetSetting("DefaultFliers");
//            return string.IsNullOrWhiteSpace(defaultIds) ? new List<BulletinFlierModel>() :
//                BulletinFlierModelUtil.IdsToModel(defaultIds.Split(','), _queryService, _blobStorage, _viewModelFactory);
//        }

        [NonAction]
        public static DefaultDetailsViewModel GetDetail(string id, GenericQueryServiceInterface queryService, FlierBehaviourQueryServiceInterface behaviourQueryService, BlobStorageInterface blobStorage, FlierBehaviourViewModelFactoryInterface viewModelFactory, PostaFlyaBrowserInformationInterface browserInformation)
        {
            
            var flier = queryService.FindByFriendlyId<Flier>(id);
            if (flier == null)
            {
                flier = queryService.FindById<Flier>(id);
                if (flier == null)
                    return null;
            }
                

            var behaviour = behaviourQueryService.GetBehaviourFor(flier);
            if (behaviour == null)
                return null;

            var ret = viewModelFactory
                .GetBehaviourViewModel(behaviour);
            ret.Flier.GetImageUrl(blobStorage);
            ret.Flier.ImageList.ForEach(_ => _.GetImageUrl(blobStorage, ThumbOrientation.Horizontal, ThumbSize.S57));

            if (browserInformation.Browser.Id == flier.BrowserId)
                AddOwnerInfo(flier, ret, queryService);

            var dets = flier.ContactDetails;
            if (dets != null)
                ret.VenueInformation = dets.ToViewModel();

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
        public static IList<BulletinFlierModel> GetFliers(FlierSearchServiceInterface flierSearchService, GenericQueryServiceInterface flierQueryService, BlobStorageInterface blobStorage, FlierBehaviourViewModelFactoryInterface viewModelFactory, LocationModel loc, 
            int count, string board = "", string skipPast = null, int distance = 0, string tags = "", DateTime? date = null)
        {
            var locDomainModel = loc.ToDomainModel();
            var tagsModel = new Tags(tags);
            var skip = string.IsNullOrWhiteSpace(skipPast) ? null : flierQueryService.FindById<Flier>(skipPast);
            if (!string.IsNullOrWhiteSpace(board))
            {
                var found = flierQueryService.FindByFriendlyId<Board>(board);
                board = found != null ? found.Id : board;
            }

            //            location.Latitude = -37.7654897;
            //            location.Longitude = 144.9770748;
            distance = Math.Min(distance, 30);
            count = Math.Min(count, 50);

            var fliersIds = string.IsNullOrWhiteSpace(board) ?
                flierSearchService.FindFliersByLocationAndDistance(locDomainModel, distance, count, skip , tagsModel, date) :
                flierSearchService.FindFliersByBoard(board, count, skip, date, tagsModel, FlierSortOrder.SortOrder, locDomainModel, distance);

            var watch = new Stopwatch();
            watch.Start();
            var ret = BulletinFlierModelUtil.IdsToModel(fliersIds, flierQueryService, blobStorage, viewModelFactory);
            Trace.TraceInformation("Bulletin Get FindById time: {0}, numfliers {1}", watch.ElapsedMilliseconds, ret.Count());
            return ret;
        }


    }
}