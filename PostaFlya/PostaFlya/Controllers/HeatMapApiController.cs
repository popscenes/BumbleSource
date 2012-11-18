using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class HeatMapApiController : ApiController
    {
        private readonly GenericQueryServiceInterface _flierQueryService;
        private readonly FlierSearchServiceInterface _flierSearchService;

        private const int RoundingForHeatmapGrouping = 4;

        public HeatMapApiController(GenericQueryServiceInterface flierQueryService, FlierSearchServiceInterface flierSearchService)
        {
            _flierQueryService = flierQueryService;
            _flierSearchService = flierSearchService;
        }

        public IQueryable<HeatMapPoint> Get([FromUri]LocationModel loc, int distance = 20, string tagstring = "")
        {
            var location = loc.ToDomainModel();
            var tags = new Tags(tagstring);
            var flierIds = _flierSearchService.FindFliersByLocationTagsAndDistance(location, tags, null, distance);

            var fliers = flierIds.Select(id => _flierQueryService.FindById<Flier>(id)).Where(f => f != null).AsQueryable();
            return GetHeatMapPointsFromFliers(fliers);
        }

        protected IQueryable<HeatMapPoint> GetHeatMapPointsFromFliers(IQueryable<FlierInterface> fliers)
        {
            var groupedFliers = fliers.GroupBy(_ => new { Longitude = Math.Round(_.Location.Longitude, RoundingForHeatmapGrouping), 
                                                          Latitude = Math.Round(_.Location.Latitude, RoundingForHeatmapGrouping) });
            var heatMapPointList = groupedFliers.Select(groupedFlier => new HeatMapPoint()
            {
                Longitude = groupedFlier.Key.Longitude, Latitude = groupedFlier.Key.Latitude
                ,
                Weight = groupedFlier.Sum(_ => 1)
            });

            return heatMapPointList;
        }

    }
}