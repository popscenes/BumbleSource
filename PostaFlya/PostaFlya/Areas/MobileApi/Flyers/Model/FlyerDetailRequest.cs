using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Website.Application.Extension.Validation;

namespace PostaFlya.Areas.MobileApi.Flyers.Model
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class FlyerDetailRequest
    {
        [DataMember(IsRequired = true)]
        [Required]
        [ConvertableToGuid]
        public string Id { get; set; }
    }
}