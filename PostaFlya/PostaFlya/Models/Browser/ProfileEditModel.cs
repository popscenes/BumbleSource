using System.ComponentModel;
using System.Web.Mvc;
using PostaFlya.Application.Domain.Location;
using WebSite.Application.Extension.Validation;
using PostaFlya.Models.Location;

namespace PostaFlya.Models.Browser
{
    [DisplayName("Profile Edit")]
    public class ProfileEditModel
    {  
        [RequiredWithMessage]
        public string Id { get; set; }   
        
        [DisplayName("Handle")]
        [StringLengthWithMessage(100, MinimumLength = 1)]
        [AlphaNumericAndUnderscoreWithMessage]
        [Remote("CheckHandle", "Profile")]
        public string Handle { get; set; }

        [DisplayName("FirstName")]
        [StringLengthWithMessage(100)]
        public string FirstName { get; set; }

        [DisplayName("MiddleNames")]
        [StringLengthWithMessage(100)]
        public string MiddleNames { get; set; }

        [DisplayName("Surname")]
        [StringLengthWithMessage(100)]
        public string Surname { get; set; }

        [DisplayName("EmailAddress")]
        [StringLengthWithMessage(100)]
        [EmailAddressWithMessage]
        public string Email { get; set; }

        [ValidLocation]
        public LocationModel Address { get; set; }
        
        [DisplayName("Allow address to be visible")]
        public bool AddressPublic { get; set; }
        
        public string AvatarImageId { get; set; } 
    }
}