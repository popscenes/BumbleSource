using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PostaFlya.Domain.Flier;
using Resources = PostaFlya.Properties.Resources;

namespace PostaFlya.Models.Flier
{
    public static class UserLinkViewModelExtensions
    {
        public static UserLinkViewModel ToCreateModel(this UserLink userLink)
        {
            //dynamic behav = flier.Behaviour;
            return new UserLinkViewModel()
                {
                    Link = userLink.Link,
                    Text = userLink.Text,
                    Type = userLink.Type
                };
        }

    }

    public class UserLinkViewModel
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Display(ResourceType = typeof(Properties.Resources), Name = "UserLinkViewModel_Type_Link_Type")] 
        public LinkType Type { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Display(ResourceType = typeof(Properties.Resources), Name = "UserLinkViewModel_Text_Link_Text")] 
        public string Text { get; set; }

        [Url(ErrorMessageResourceName = "ValidUrl", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Display(ResourceType = typeof(Properties.Resources), Name = "UserLinkViewModel_Link_Link_Url")] 
        public string Link { get; set; }

        
    }
}