using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostaFlya.Domain.Venue;
using Website.Application.Domain.Google.Places;
using Website.Application.Google.Places.Details;
using Website.Domain.Location;
using Location = Website.Domain.Location.Location;

namespace PostaFlya.Application.Domain.Google.Places
{
    public static class VenueInformationExtensions
    {
        public static VenueInformation MapFrom(this VenueInformation ret, Result googlePlaces)
        {
            ret.PhoneNumber = googlePlaces.formatted_phone_number;
            ret.PlaceName = googlePlaces.name;
            ret.Source = "Google Place";
            ret.SourceId = googlePlaces.reference;
            ret.SourceUrl = googlePlaces.url;
            ret.UtcOffset = googlePlaces.utc_offset;
            ret.WebSite = googlePlaces.website;
            ret.Address = new Location().MapFrom(googlePlaces);
            return ret;
        }
    }
}
