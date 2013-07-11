using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using PostaFlya.Models.Flier;
using Website.Application.Extension.Validation;

namespace PostaFlya.Areas.MobileApi.Flyers.Model
{
    [DataContract]
    public class FlyersByBoardRequest : RequestModelInterface
    {

        [DataMember(IsRequired = true)]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [ConvertableToGuid]
        public string BoardId { get; set; }

        [DataMember(IsRequired = false)]
        public DateTimeOffset Start { get; set; }

        [DataMember(IsRequired = false)]
        public DateTimeOffset End { get; set; }
    }
}