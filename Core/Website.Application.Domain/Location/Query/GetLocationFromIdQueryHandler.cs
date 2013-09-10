using Website.Domain.Location;
using Website.Domain.Location.Query;
using Website.Infrastructure.Query;

namespace Website.Application.Domain.Location.Query
{
    public class GetLocationFromIdQueryHandler : QueryHandlerInterface<GetLocationFromIdQuery, Website.Domain.Location.Suburb>
    {
        private readonly GeoIpServiceInterface _geoIpService;
        private readonly QueryChannelInterface _queryChannel;

        public GetLocationFromIdQueryHandler(GeoIpServiceInterface geoIpService, QueryChannelInterface queryChannel)
        {
            _geoIpService = geoIpService;
            _queryChannel = queryChannel;
        }

        public Website.Domain.Location.Suburb Query(GetLocationFromIdQuery argument)
        {
             var loc = _geoIpService.GetLocationForIp(argument.IpAddress);
            if (loc == null) return null;

            var res = _queryChannel.Query(new FindNearestSuburbByGeoCoordsQuery()
            {
                Geo = new GeoCoords() { Latitude = loc.Latitude, Longitude = loc.Longitude }

            }, default(Suburb));
            return res;
        }
    }
}