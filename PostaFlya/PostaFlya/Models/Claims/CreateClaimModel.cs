using System.ComponentModel;
using System.Runtime.Serialization;
using Website.Application.Extension.Validation;
using PostaFlya.Binding;

namespace PostaFlya.Models.Claims
{
    [DataContract]
    public class CreateClaimModel
    {
        [RequiredWithMessage]
        [DataMember(IsRequired = true)]
        public EntityTypeEnum ClaimEntity { get; set; }
            
        [RequiredWithMessage]
        [DataMember]
        public string EntityId { get; set; }
        
        [RequiredWithMessage]
        [DataMember]
        public string BrowserId { get; set; }
    }
}