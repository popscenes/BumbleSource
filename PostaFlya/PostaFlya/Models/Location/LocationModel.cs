﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Web.Http.ModelBinding;
using PostaFlya.Areas.WebApi.Location.Model;
using PostaFlya.Models.Board;
using Website.Application.Google.Places.Details;
using Website.Common.Model;
using Website.Domain.Location;

namespace PostaFlya.Models.Location
{
    public class ToLocationModel : ViewModelMapperInterface<LocationModel, Website.Domain.Location.Location>
    {
        public LocationModel ToViewModel(LocationModel target, Website.Domain.Location.Location source)
        {
            if (source == null)
                return new LocationModel();

            var ret = target ?? new LocationModel();
            ret.CopyFieldsFrom(source);
            return ret;
        }
    }

    public static class BulletinFlierModelFlierInterfaceExtension
    {
        public static LocationModel ToViewModel(this Website.Domain.Location.Location location)
        {
            if(location == null)
                return new LocationModel();

            var ret = new LocationModel();
            ret.CopyFieldsFrom(location);
            return ret;
        }
        
    }

    [Serializable]
    [DataContract]
    public class LocationModel : LocationAndAddressInterface, IsModelInterface
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

        [DataMember]
        [Display(Name = "Longitude", ResourceType = typeof(Properties.Resources))] 
        public double Longitude { get; set; }

        [DataMember]
        [Display(Name = "Latitude", ResourceType = typeof(Properties.Resources))] 
        public double Latitude { get; set; }


        [DataMember]
        [Display(Name = "StreetNumber", ResourceType = typeof(Properties.Resources))]
        public string StreetNumber { get; set; }

        [DataMember]
        [Display(Name = "Street", ResourceType = typeof(Properties.Resources))] 
        public string Street { get; set; }

        [DataMember]
        [Display(Name = "Locality", ResourceType = typeof(Properties.Resources))] 
        public string Locality { get; set; }

        [DataMember]
        [Display(Name = "Region", ResourceType = typeof(Properties.Resources))] 
        public string Region { get; set; }

        [DataMember]
        public string RegionCode { get; set; }

        [DataMember]
        public string CountryCode { get; set; }

        [DataMember]
        [Display(Name = "PostCode", ResourceType = typeof(Properties.Resources))] 
        public string PostCode { get; set; }

        [DataMember]
        [Display(Name = "CountryName", ResourceType = typeof(Properties.Resources))] 
        public string CountryName { get; set; }

    }
}