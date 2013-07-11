using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Website.Application.Extension.Validation;
using PostaFlya.Binding;
using Website.Common.Model;
using Website.Domain.Browser;

namespace PostaFlya.Models.Claims
{
    [DataContract]
    public class CreateClaimModel : BrowserIdInterface, IsModelInterface
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DataMember(IsRequired = true)]
        public EntityTypeEnum ClaimEntity { get; set; }
            
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DataMember]
        public string EntityId { get; set; }
        
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DataMember]
        public string BrowserId { get; set; }

        [DataMember]
        public string ClaimerMessage { get; set; }
    }
}