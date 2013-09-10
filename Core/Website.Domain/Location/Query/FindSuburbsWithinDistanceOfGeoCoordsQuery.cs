using System.Collections.Generic;
using Website.Infrastructure.Query;

namespace Website.Domain.Location.Query
{
    public class FindSuburbsWithinDistanceOfGeoCoordsQuery : QueryInterface<Suburb>
    {
        public int Kilometers { get; set; }
        public GeoCoords Geo { get; set; }
    }
}