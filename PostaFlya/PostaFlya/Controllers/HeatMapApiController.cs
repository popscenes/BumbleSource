//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web.Http;
//using PostaFlya.Domain.Boards;
//using PostaFlya.Domain.Flier.Query;
//using PostaFlya.Domain.Flier;
//using PostaFlya.Models.Flier;
//using PostaFlya.Models.Location;
//using Website.Common.Obsolete;
//using Website.Domain.Tag;
//using Website.Infrastructure.Query;
//
//namespace PostaFlya.Controllers
//{
//    public class HeatMapApiController : OldWebApiControllerBase
//    {
//        private readonly GenericQueryServiceInterface _flierQueryService;
//        private readonly FlierSearchServiceInterface _flierSearchService;
//        private readonly QueryChannelInterface _queryChannel;
//
//        private const int RoundingForHeatmapGrouping = 4;
//
//        public HeatMapApiController(GenericQueryServiceInterface flierQueryService, FlierSearchServiceInterface flierSearchService, QueryChannelInterface queryChannel)
//        {
//            _flierQueryService = flierQueryService;
//            _flierSearchService = flierSearchService;
//            _queryChannel = queryChannel;
//        }
//
//        public IQueryable<HeatMapPoint> Get([FromUri]LocationModel loc, int distance = 20, string tagstring = "")
//        {
//            var location = loc.ToDomainModel();
//            var tags = new Tags(tagstring);
//            var flierIds = _flierSearchService.FindFliersByLocationAndDistance(location, distance: distance, take: 1000, tags: tags);
//
//            var fliers = flierIds.Select(id => _flierQueryService.FindById<Flier>(id)).Where(f => f != null).AsQueryable();
//            return GetHeatMapPointsFromFliers(fliers);
//        }
//
//        protected IQueryable<HeatMapPoint> GetHeatMapPointsFromFliers(IQueryable<FlierInterface> fliers)
//        {
//            var flyerAndLoc = fliers.Select(f => new
//                {
//                    flyer = f,
//                    loc =  _queryChannel.Query(new GetFlyerVenueBoardQuery() { FlyerId = f.Id }, (Board)null).Venue().Address
//                });
//            
//            var groupedFliers = flyerAndLoc.GroupBy(_ => new
//            {
//                Longitude = Math.Round(_.loc.Longitude, RoundingForHeatmapGrouping),
//                Latitude = Math.Round(_.loc.Latitude, RoundingForHeatmapGrouping)
//            });
//
//            var heatMapPointList = groupedFliers.Select(groupedFlier => new HeatMapPoint()
//            {
//                Longitude = groupedFlier.Key.Longitude, Latitude = groupedFlier.Key.Latitude
//                ,
//                Weight = groupedFlier.Sum(_ => 1)
//            });
//
//            return heatMapPointList;
//        }
//
//    }
//}