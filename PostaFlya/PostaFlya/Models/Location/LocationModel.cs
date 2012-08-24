using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;
using Website.Domain.Location;

namespace PostaFlya.Models.Location
{
    public static class BulletinFlierModelFlierInterfaceExtension
    {
        public static LocationModel ToViewModel(this Website.Domain.Location.Location location)
        {
            if(location == null)
                return new LocationModel();

            return new LocationModel()
                       {
                           Longitude = location.Longitude,
                           Latitude = location.Latitude,
                           Description = location.Description
                       };
        }
    }

    public class LocationModel : LocationInterface
    {
        public LocationModel(LocationInterface loc)
        {
            Longitude = loc.Longitude;
            Latitude = loc.Latitude;
        }

        public LocationModel():
            this(new Website.Domain.Location.Location())
        {
            //set to invalid on creation
        }

        public Website.Domain.Location.Location ToDomainModel()
        {
            return new Website.Domain.Location.Location(Longitude, Latitude) {Description = Description };
        }

        [DisplayName("Longitude")]
        public double Longitude { get; set; }
        [DisplayName("Latitude")]
        public double Latitude { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }  
    }
}