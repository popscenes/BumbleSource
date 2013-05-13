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
        public static Website.Domain.Location.Location MapFrom(this Website.Domain.Location.Location current, Address_Components[] googleComponents)
        {
            
            return current;
        }

        public static Website.Domain.Location.Location MapFrom(this Website.Domain.Location.Location current, Geometry googleComponents)
        {

            return current;
        }

        public static Website.Domain.Location.Location MapFrom(this Website.Domain.Location.Location current,
                                                               Result googlePlaces)
        {
            return current.MapFrom(googlePlaces.address_components).MapFrom(googlePlaces.geometry);
        }
    }
}
