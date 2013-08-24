using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using MaxMind.GeoIP;

namespace Website.Application.Domain.Location
{
    public interface GeoIpServiceInterface
    {
        Website.Domain.Location.Location GetLocationForIp(string ipAddress);
    }

    class MaxMindGeoIpService : GeoIpServiceInterface
    {
        private readonly LookupService _service;

        public MaxMindGeoIpService(HttpContextBase httpContext)
        {
            var dataFile = httpContext.Server.MapPath("~/App_Data/GeoLiteCity.dat");
            _service = new LookupService(dataFile, LookupService.GEOIP_MEMORY_CACHE);
        }

        public Website.Domain.Location.Location GetLocationForIp(string ipAddress)
        {
            var location = _service.getLocation(ipAddress);
            if (location == null) return null;

            return new Website.Domain.Location.Location()
                {
                    CountryName = location.countryName,
                    Longitude = location.longitude,
                    Latitude = location.latitude,
                    PostCode = location.postalCode,
                    Region = location.regionName
                };
        }
    }
}
