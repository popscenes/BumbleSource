using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Website.Common.Model;
using Website.Domain.Location;
using Res = PostaFlya.Properties.Resources;


namespace PostaFlya.Areas.WebApi.Location.Model
{
    public class ToSuburbModel : 
        ViewModelMapperInterface<SuburbModel, Suburb>,
        ViewModelMapperInterface<Suburb, SuburbModel>

    {
        public SuburbModel ToViewModel(SuburbModel target, Suburb source)
        {
            if (target == null)
                target = new SuburbModel();
            target.CopyFieldsFrom(source);
            target.Id = source.Id;
            return target;
        }

        public Suburb ToViewModel(Suburb target, SuburbModel source)
        {
            if (target == null)
                target = new Suburb();
            target.CopyFieldsFrom(source);
            target.Id = source.Id;
            return target;
        }
    }

    [Serializable]
    [DataContract]
    public class SuburbModel : IsModelInterface, SuburbInterface 
    {
        [DataMember]
        [Display(Name = "Id", ResourceType = typeof(Res))]
        public string Id { get; set; }

        [DataMember]
        [Display(Name = "Longitude", ResourceType = typeof(Res))]
        public double Longitude { get; set; }

        [DataMember]
        [Display(Name = "Latitude", ResourceType = typeof(Res))]        
        public double Latitude { get; set; }

        [DataMember]
        [Display(Name = "Locality", ResourceType = typeof(Res))]
        public string Locality { get; set; }

        [DataMember]
        [Display(Name = "Region", ResourceType = typeof(Res))]
        public string Region { get; set; }
        [DataMember]
        public string RegionCode { get; set; }

        [DataMember]
        [Display(Name = "CountryName", ResourceType = typeof(Res))]
        public string CountryName { get; set; }
        [DataMember]
        public string CountryCode { get; set; }


        [Display(Name = "PostCode", ResourceType = typeof(Res))]
        public string PostCode { get; set; }
    }
}