using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using PostaFlya.Models.Flier;
using Website.Application.Extension.Validation;

namespace PostaFlya.Areas.WebApi.Flyers.Model
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class FlyerDetailRequest : RequestModelInterface
    {
        [DataMember(IsRequired = true)]
        [Required]
        [ConvertableToGuid]
        public string Id { get; set; }
    }
}