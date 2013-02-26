﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

            var ret = new LocationModel();
            ret.CopyFieldsFrom(location);
            ret.Description = location.Description;
            return ret;
        }
    }

    public class LocationModel : LocationAndAddressInterface
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
            var ret = new Website.Domain.Location.Location(Longitude, Latitude);
            ret.CopyFieldsFrom((LocationInterface) this);
            ret.CopyFieldsFrom((AddressInterface)this);
            return ret;
        }

        [Display(Name = "Longitude", ResourceType = typeof(Properties.Resources))] 
        public double Longitude { get; set; }
        [Display(Name = "Latitude", ResourceType = typeof(Properties.Resources))] 
        public double Latitude { get; set; }

        [Display(Name = "Description", ResourceType = typeof(Properties.Resources))] 
        public string Description { get; set; }

        [Display(Name = "PlaceName", ResourceType = typeof (Properties.Resources))]
        public string PlaceName { get; set; }
        [Display(Name = "StreetNumber", ResourceType = typeof(Properties.Resources))]
        public string StreetNumber { get; set; }
        [Display(Name = "Street", ResourceType = typeof(Properties.Resources))] 
        public string Street { get; set; }
        [Display(Name = "Locality", ResourceType = typeof(Properties.Resources))] 
        public string Locality { get; set; }
        [Display(Name = "Region", ResourceType = typeof(Properties.Resources))] 
        public string Region { get; set; }
        [Display(Name = "PostCode", ResourceType = typeof(Properties.Resources))] 
        public string PostCode { get; set; }
        [Display(Name = "CountryName", ResourceType = typeof(Properties.Resources))] 
        public string CountryName { get; set; }

    }
}