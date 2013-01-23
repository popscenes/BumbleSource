using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Website.Application.Extension.Validation;
using PostaFlya.Models.Location;
using Website.Application.Domain.Location;
using Resources = PostaFlya.Properties.Resources;

namespace PostaFlya.Models.Browser
{
    public class ProfileEditModel
    {  
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string Id { get; set; }   
        
        [Display(Name = "Handle", ResourceType = typeof(Properties.Resources))]
        [StringLength(100, MinimumLength = 1, ErrorMessageResourceName = "StringTooLargeOrSmall", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [AlphaNumericAndHiphen(ErrorMessageResourceName = "AlpaNumericAndHiphensOnly", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Remote("CheckHandle", "Profile")]
        public string Handle { get; set; }

        
        [Display(Name = "FirstName", ResourceType = typeof(Properties.Resources))]
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string FirstName { get; set; }

        [Display(Name = "MiddleNames", ResourceType = typeof(Properties.Resources))]         
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string MiddleNames { get; set; }

        [Display(Name = "Surname", ResourceType = typeof(Properties.Resources))] 
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string Surname { get; set; }

        [Display(Name = "Credits", ResourceType = typeof(Properties.Resources))]
        public double Credits { get; set; }      

        [Display(Name = "EmailAddress", ResourceType = typeof(Properties.Resources))] 
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [EmailAddress(ErrorMessageResourceName = "InvalidEmailAddress", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string Email { get; set; }

        [ValidLocation(ErrorMessageResourceName = "ValidLocation", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Display(Name = "Address", ResourceType = typeof(Properties.Resources))]
        public LocationModel Address { get; set; }

        [UrlAttribute(ErrorMessageResourceName = "ValidUrl", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string WebSite { get; set; }
        
        public string AvatarImageId { get; set; } 
    }
}