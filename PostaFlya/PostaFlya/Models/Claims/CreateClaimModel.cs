using System.ComponentModel;
using Website.Application.Extension.Validation;
using PostaFlya.Binding;

namespace PostaFlya.Models.Claims
{
    public class CreateClaimModel
    {
        [RequiredWithMessage]
        public EntityTypeEnum ClaimEntity { get; set; }
            
        [RequiredWithMessage]
        public string EntityId { get; set; }
        
        [RequiredWithMessage]
        public string BrowserId { get; set; }
    }
}