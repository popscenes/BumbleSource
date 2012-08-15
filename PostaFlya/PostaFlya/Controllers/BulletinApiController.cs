using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.Http;
using System.Web.Http.Routing;
using WebSite.Application.Binding;
using PostaFlya.Application.Domain.Content;
using PostaFlya.Areas.Default.Models;
using PostaFlya.Areas.Default.Models.Bulletin;
using PostaFlya.Domain.Behaviour.Query;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.Tag;
using PostaFlya.Helpers;
using PostaFlya.Models.Content;
using PostaFlya.Models.Factory;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using PostaFlya.Models.Tags;
using WebSite.Application.Content;

namespace PostaFlya.Controllers
{
    public class BulletinApiController : ApiController
    {
        private readonly FlierQueryServiceInterface _flierQueryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierBehaviourQueryServiceInterface _behaviourQueryService;
        private readonly FlierBehaviourViewModelFactoryInterface _viewModelFactory;

        public BulletinApiController(FlierQueryServiceInterface flierQueryService,
            [ImageStorage]BlobStorageInterface blobStorage
            , FlierBehaviourQueryServiceInterface behaviourQueryService
            , FlierBehaviourViewModelFactoryInterface viewModelFactory)
        {
            _flierQueryService = flierQueryService;
            _blobStorage = blobStorage;
            _behaviourQueryService = behaviourQueryService;
            _viewModelFactory = viewModelFactory;
        }

        public DefaultDetailsViewModel Get(string id)
        {
            return GetDetail(id, _flierQueryService, _behaviourQueryService, _blobStorage, _viewModelFactory);
        }

        public IList<BulletinFlierModel> Get([FromUri]LocationModel loc
            ,int count, int skip = 0, int distance = 0, string tags = "")
        {
            return GetFliers(_flierQueryService, _blobStorage, _viewModelFactory
                             , loc, count, skip, distance, tags);
        }

        [NonAction]
        public static DefaultDetailsViewModel GetDetail(string id
            , FlierQueryServiceInterface flierQueryService
            , FlierBehaviourQueryServiceInterface behaviourQueryService
            , BlobStorageInterface blobStorage
            , FlierBehaviourViewModelFactoryInterface viewModelFactory)
        {
            
            var flier = flierQueryService.FindById(id);
            if (flier == null)
                return null;

            var behaviour = behaviourQueryService.GetBehaviourFor(flier);
            if (behaviour == null)
                return null;

            var ret = viewModelFactory
                .GetBehaviourViewModel(behaviour);
            ret.Flier.GetImageUrl(blobStorage);
            ret.Flier.ImageList.ForEach(_ => _.GetDefaultImageUrl(blobStorage, ThumbOrientation.Vertical, ThumbSize.S50));
            return ret;
        }


        [NonAction]
        public static IList<BulletinFlierModel> GetFliers(FlierQueryServiceInterface flierQueryService
            , BlobStorageInterface blobStorage, FlierBehaviourViewModelFactoryInterface viewModelFactory
            , LocationModel loc, int count, int skip = 0, int distance = 0, string tags = "")
        {
            Location locDomainModel = loc.ToDomainModel();
            var tagsModel = new Tags(tags);

            //            location.Latitude = -37.7654897;
            //            location.Longitude = 144.9770748;
            distance = Math.Min(distance, 30);
            count = Math.Min(count, 50);

            var fliersIds =
                flierQueryService.FindFliersByLocationTagsAndDistance(locDomainModel,
                tagsModel, distance, count, FlierSortOrder.CreatedDate, skip);

            var watch = new Stopwatch();
            watch.Start();
            var ret = fliersIds
                .Select(f => viewModelFactory.GetBulletinViewModel(flierQueryService.FindById(f), false)
                    .GetImageUrl(blobStorage))
                .ToList();
            Trace.TraceInformation("Bulletin Get FindById time: {0}, numfliers {1}", watch.ElapsedMilliseconds, ret.Count());
            return ret;
        }
    }
}