using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using PostaFlya.Models.Flier;

namespace PostaFlya.Areas.WebApi.Location.Model
{
    [DataContract]
    public class AutoCompleteRequest : RequestModelInterface
    {
        [DataMember(IsRequired = true)]
        [Required]
        [StringLength(50, ErrorMessageResourceName = "StringTooLargeOrSmall", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null, MinimumLength = 3)]
        public string Q { get; set; }
    }
}