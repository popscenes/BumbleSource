using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using Website.Application.Domain.Location;

namespace PostaFlya.Areas.WebApi.Flyers.Model
{
    //lat=-37.769&long=144.979&distance=10&take=30 
    [DataContract]
    public class FlyersByLocationRequest : RequestModelInterface
    {
        public FlyersByLocationRequest()
        {
            Distance = 30;
        }

//        [Range(-90.0, 90.0)]
//        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
//        [DataMember(IsRequired = true)]
//        public double Lat { get; set; }
//
//        [Range(-180.0, 180.0)]
//        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
//        [DataMember(IsRequired = true)]
//        public double Lng { get; set; }

        [ValidLocation]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DataMember(IsRequired = true)]
        public LocationModel Loc { get; set; }

        [Range(0, 30)]
        [DataMember(IsRequired = false)]      
        public int Distance { get; set; }

        [DataMember(IsRequired = false)] 
        public DateTimeOffset Start { get; set; }

        [DataMember(IsRequired = false)]
        public DateTimeOffset End { get; set; }
    }
}