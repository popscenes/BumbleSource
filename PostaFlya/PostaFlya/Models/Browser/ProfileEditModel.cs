using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Website.Application.Extension.Validation;
using PostaFlya.Models.Location;
using Website.Application.Domain.Location;
namespace PostaFlya.Models.Browser
{
    public class ProfileEditModel
    {  
        [RequiredWithMessage]
        public string Id { get; set; }   
        
        [Display(Name = "Handle", ResourceType = typeof(Properties.Resources))]
        [StringLengthWithMessage(100, MinimumLength = 1)]
        [AlphaNumericAndUnderscoreWithMessage]
        [Remote("CheckHandle", "Profile")]
        public string Handle { get; set; }

        
        [Display(Name = "FirstName", ResourceType = typeof(Properties.Resources))] 
        [StringLengthWithMessage(100)]
        public string FirstName { get; set; }

        [Display(Name = "MiddleNames", ResourceType = typeof(Properties.Resources))]         
        [StringLengthWithMessage(100)]
        public string MiddleNames { get; set; }

        [Display(Name = "Surname", ResourceType = typeof(Properties.Resources))] 
        [StringLengthWithMessage(100)]
        public string Surname { get; set; }

        [Display(Name = "Credits", ResourceType = typeof(Properties.Resources))]
        public double Credits { get; set; }      

        [Display(Name = "EmailAddress", ResourceType = typeof(Properties.Resources))] 
        [StringLengthWithMessage(100)]
        [EmailAddressWithMessage]
        public string Email { get; set; }

        [ValidLocation]
        public LocationModel Address { get; set; }

        [Display(Name = "AddressPublic", ResourceType = typeof(Properties.Resources))] 
        public bool AddressPublic { get; set; }
        
        public string AvatarImageId { get; set; } 
    }
}