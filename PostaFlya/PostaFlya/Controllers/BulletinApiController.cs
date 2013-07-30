using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Application.Domain.Flier;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier.Analytic;
using Website.Application.Binding;
using PostaFlya.Areas.Default.Models;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using Website.Application.Content;
using Website.Common.Model.Query;
using Website.Common.Obsolete;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class BulletinApiController : OldWebApiControllerBase
    {
        private readonly GenericQueryServiceInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierSearchServiceInterface _flierSearchService;
        private readonly FlierWebAnalyticServiceInterface _webAnalyticService;
        private readonly TinyUrlServiceInterface _tinyUrlService;
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;
        private readonly ConfigurationServiceInterface _configurationService;
        private readonly QueryChannelInterface _queryChannel;


        public BulletinApiController(GenericQueryServiceInterface queryService,
            [ImageStorage]BlobStorageInterface blobStorage, FlierSearchServiceInterface flierSearchService
            , FlierWebAnalyticServiceInterface webAnalyticService, TinyUrlServiceInterface tinyUrlService
            , PostaFlyaBrowserInformationInterface browserInformation, ConfigurationServiceInterface configurationService, QueryChannelInterface queryChannel)
        {
            _queryService = queryService;
            _blobStorage = blobStorage;
            _flierSearchService = flierSearchService;
            _webAnalyticService = webAnalyticService;
            _tinyUrlService = tinyUrlService;
            _browserInformation = browserInformation;
            _configurationService = configurationService;
            _queryChannel = queryChannel;
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
            var ret = GetDetail(id, _queryChannel);
            
            if(ret != null)
                _webAnalyticService.RecordVisit(ret.Flier.Id, FlierAnalyticSourceAction.IdByApi);
            
            return ret;
        }

        /// <summary>
        /// dgdgsdgdsfgg
        /// </summary>
        /// <param name="req">ytrydyr</param>
        /// <returns></returns>
        public IList<BulletinFlierSummaryModel> Get([FromUri]BulletinGetRequestModel req)
        {
//            if (!loc.IsValid() && string.IsNullOrWhiteSpace(board))
//                return GetDefaultFliers(count, skipPast, tags);

            if (_browserInformation.LastSearchLocation == null)
            {
                _browserInformation.LastSearchLocation = new Location();
            }
            _browserInformation.LastSearchLocation.CopyFieldsFrom(req.Loc);

            _webAnalyticService.SetLastSearchLocation(req.Loc.ToDomainModel());
            return GetFliers(_flierSearchService, _queryChannel, _queryService 
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
        public static DefaultDetailsViewModel GetDetail(string id, QueryChannelInterface queryChannel)
        {
            var flier = queryChannel.Query(new FindByFriendlyIdQuery<Flier>() { FriendlyId = id }, (BulletinFlierDetailModel)null);

            if (flier == null)
            {
                flier = queryChannel.Query(new FindByIdQuery<Flier>() { Id = id }, (BulletinFlierDetailModel)null);

                if (flier == null)
                    return null;
            }

            return new DefaultDetailsViewModel() { Flier = flier };
        }




        [NonAction]
        public static IList<BulletinFlierSummaryModel> GetFliers(FlierSearchServiceInterface flierSearchService
            , QueryChannelInterface queryChannel 
            , GenericQueryServiceInterface flierQueryService, LocationModel loc, 
            int count, string board = "", string skipPast = null, int distance = 0, string tags = "", DateTime? date = null)
        {
            var locDomainModel = loc.ToDomainModel();
            var tagsModel = new Tags(tags);
            var skip = string.IsNullOrWhiteSpace(skipPast) ? null : flierQueryService.FindById<Flier>(skipPast);
            if (!string.IsNullOrWhiteSpace(board))
            {
                var found = queryChannel.Query(new FindByFriendlyIdQuery<Board>() { FriendlyId = board }, (Board)null);
                board = found != null ? found.Id : board;
            }

            //            location.Latitude = -37.7654897;
            //            location.Longitude = 144.9770748;
            distance = Math.Min(distance, 30);
            count = Math.Min(count, 50);

            var watch = new Stopwatch();
            watch.Start();

            var fliersIds = string.IsNullOrWhiteSpace(board) ?
                flierSearchService.FindFliersByLocationAndDistance(locDomainModel, distance, count, skip , tagsModel, date) :
                flierSearchService.FindFliersByBoard(board, count, skip, date, tagsModel, FlierSortOrder.SortOrder, locDomainModel, distance);


            var fliers = flierQueryService.FindByIds<PostaFlya.Domain.Flier.Flier>(fliersIds);
            var ret = queryChannel.ToViewModel<BulletinFlierSummaryModel, Flier>(fliers);
            Trace.TraceInformation("Bulletin Get FindById time: {0}, numfliers {1}", watch.ElapsedMilliseconds, ret.Count());
            return ret;
        }


    }
}