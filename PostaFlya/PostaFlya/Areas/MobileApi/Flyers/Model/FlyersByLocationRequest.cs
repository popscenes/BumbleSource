﻿using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using PostaFlya.Models.Flier;
using Website.Application.Domain.Location;

namespace PostaFlya.Areas.MobileApi.Flyers.Model
{
    //lat=-37.769&long=144.979&distance=10&take=30 
    [DataContract]
    public class FlyersByLocationRequest : RequestModelInterface
    {
        public FlyersByLocationRequest()
        {
            Distance = 5;
            Take = 30;
        }

        [Range(-90.0, 90.0)]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DataMember(IsRequired = true)]
        public double Lat { get; set; }

        [Range(-180.0, 180.0)]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DataMember(IsRequired = true)]
        public double Long { get; set; }

        [Range(0, 30)]
        [DataMember(IsRequired = true)]      
        public int Distance { get; set; }

        [DataMember]
        [Range(0, 100)]
        public int Take { get; set; }

        [DataMember]
        public string Skip { get; set; }
    }
}