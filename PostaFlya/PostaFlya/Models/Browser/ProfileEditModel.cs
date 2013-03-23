using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Website.Application.Extension.Validation;
using PostaFlya.Models.Location;
using Website.Application.Domain.Location;
using Resources = PostaFlya.Properties.Resources;

namespace PostaFlya.Models.Browser
{
    public class ProfileEditModel : ContactDetailsModel, PageModelInterface
    {  
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        public string Id { get; set; }   
        
        [Display(Name = "Handle", ResourceType = typeof(Properties.Resources))]
        [StringLength(100, MinimumLength = 1, ErrorMessageResourceName = "StringTooLargeOrSmall", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [AlphaNumericAndHiphen(ErrorMessageResourceName = "AlpaNumericAndHiphensOnly", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Remote("CheckHandle", "Profile")]
        public string Handle { get; set; }
        
        [Display(Name = "Credits", ResourceType = typeof(Properties.Resources))]
        public double Credits { get; set; }      
        
        [Display(ResourceType = typeof (Website.Application.Properties.Resources), Name = "ProfileEditModel_AvatarImageId_Profile_Picture")]
        public string AvatarImageId { get; set; }

        public string PageId { get; set; }
        public string ActiveNav { get; set; }
    }
}