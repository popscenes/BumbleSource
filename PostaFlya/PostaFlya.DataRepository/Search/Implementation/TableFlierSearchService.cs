using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Ninject;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Flier;
using PostaFlya.DataRepository.Search.Services;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.Tag;

namespace PostaFlya.DataRepository.Search.Implementation
{
    class TableFlierSearchService : FlierSearchServiceInterface
    {
        private readonly AzureTableContext _tableContext;
        private readonly LocationServiceInterface _locationService;

        public TableFlierSearchService([Named("flier")]AzureTableContext tableContext,
            LocationServiceInterface  locationService)
        {
            _tableContext = tableContext;
            _locationService = locationService;
        }

        public void NotifyUpdate(IEnumerable<FlierInterface> values)
        {
            
        }

        public void NotifyDelete(IEnumerable<FlierInterface> values)
        {
            
        }

        public IList<string> FindFliersByLocationTagsAndDistance(Location location, Tags tags, int distance = 0, int take = 0, FlierSortOrder sortOrder = FlierSortOrder.CreatedDate, int skip = 0)
        {
            var boundingBox = distance > 0
                                  ? _locationService.GetBoundingBox(location, distance)
                                  : _locationService.GetDefaultBox(location);
            var watch = new Stopwatch();
            watch.Start();
            Expression<Func<FlierTableEntry, bool>> query =
                (fliers) => fliers.LocationLongitude >= boundingBox.Min.Longitude
                            && fliers.LocationLongitude <= boundingBox.Max.Longitude
                            && fliers.LocationLatitude >= boundingBox.Min.Latitude
                            && fliers.LocationLatitude <= boundingBox.Max.Latitude;

            var tableEntity = _tableContext.PerformQuery(query).AsEnumerable();
            var time = watch.ElapsedMilliseconds;
            Trace.TraceInformation("FindFliers time: {0}, numfliers {1}", time, tableEntity.Count());

            watch.Restart();
            var ret = tableEntity
                .Select(ts => ts.CreateEntityCopy<Domain.Flier.Flier, FlierInterface>())
                .Distinct()
                .Where(_ => tags == null || !tags.Any() || _.Tags.IsSupersetOf(tags));

            ret = ret.OrderByDescending(GetSorter(sortOrder)).AsEnumerable();

            //skip previous fliers
            if(skip > 0)
            {
                ret = ret.Skip(skip);
            }
            
            if (take > 0)
                ret = ret.Take(take);

            time = watch.ElapsedMilliseconds;
            Trace.TraceInformation("FindFliers transform time: {0}, numfliers {1}", time, tableEntity.Count());
            return ret.Select(f => f.Id).ToList();
        }

        public static Func<FlierInterface, object> GetSorter(FlierSortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case FlierSortOrder.CreatedDate:
                    return entry => entry.CreateDate.Ticks.ToString("D20") + '[' + (entry.NumberOfLikes + entry.NumberOfComments).ToString("D10") + ']';
                case FlierSortOrder.EffectiveDate:
                    return entry => entry.EffectiveDate.Ticks.ToString("D20") + '[' + (entry.NumberOfLikes + entry.NumberOfComments).ToString("D10") + ']';
                case FlierSortOrder.Popularity:
                    return entry => (entry.NumberOfLikes + entry.NumberOfComments).ToString("D10") + '[' + entry.CreateDate.Ticks.ToString("D20") + ']';
            }
            return entry => entry.CreateDate;
        }
    }
}
