using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Application.Google.Places.Details;

namespace Website.Application.Domain.Google.Places
{
    public static class LocationExtensions
    {
        public static Website.Domain.Location.Location MapFrom(this Website.Domain.Location.Location current, Address_Components[] data)
        {
            foreach (var addressPart in data)
            {
                if (addressPart.types.Contains("street_number")) {
                    current.StreetNumber = addressPart.long_name;
                }
                else if (addressPart.types.Contains("route")) {
                    current.Street = addressPart.long_name;
                }
                else if (addressPart.types.Contains("locality")) {
                    current.Locality = addressPart.long_name;
                }
                else if (addressPart.types.Contains("administrative_area_level_1")) {
                    current.Region = addressPart.long_name;
                }
                else if (addressPart.types.Contains("country")) {
                    current.CountryName = addressPart.long_name;
                }
                else if (addressPart.types.Contains("postal_code")) {
                    current.PostCode = addressPart.long_name;
                }
            }
            return current;
        }

        public static Website.Domain.Location.Location MapFrom(this Website.Domain.Location.Location current, Geometry googleComponents)
        {
            current.Latitude = googleComponents.location.lat;
            current.Longitude = googleComponents.location.lng;
            return current;
        }

        public static Website.Domain.Location.Location MapFrom(this Website.Domain.Location.Location current,
                                                               Result googlePlaces)
        {
            return current.MapFrom(googlePlaces.address_components).MapFrom(googlePlaces.geometry);
        }
    }
}
