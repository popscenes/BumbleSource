using Website.Infrastructure.Query;

namespace Website.Domain.Location.Query
{
    public class FindNearestSuburbByGeoCoordsQuery : QueryInterface<Suburb>
    {
        public GeoCoords Geo { get; set; }
    }
}