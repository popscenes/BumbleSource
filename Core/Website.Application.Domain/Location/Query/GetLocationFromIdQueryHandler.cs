using Website.Infrastructure.Query;

namespace Website.Application.Domain.Location.Query
{
    public class GetLocationFromIdQueryHandler : QueryHandlerInterface<GetLocationFromIdQuery, Website.Domain.Location.Location>
    {
        private readonly GeoIpServiceInterface _geoIpService;

        public GetLocationFromIdQueryHandler(GeoIpServiceInterface geoIpService)
        {
            _geoIpService = geoIpService;
        }

        public Website.Domain.Location.Location Query(GetLocationFromIdQuery argument)
        {
            return _geoIpService.GetLocationForIp(argument.IpAddress);
        }
    }
}