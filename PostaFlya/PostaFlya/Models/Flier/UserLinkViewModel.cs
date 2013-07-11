using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using PostaFlya.Domain.Flier;
using Website.Common.Model;
using Resources = PostaFlya.Properties.Resources;

namespace PostaFlya.Models.Flier
{
    public class ToUserLinkViewModel : ViewModelMapperInterface<UserLinkViewModel, UserLink>
    {
        public UserLinkViewModel ToViewModel(UserLinkViewModel target, UserLink source)
        {
            if(target == null)
                target = new UserLinkViewModel();
            target.Link = source.Link;
            target.Text = source.Text;
            target.Type = source.Type.ToString(CultureInfo.InvariantCulture);
            return target;
        }
    }

    public class UserLinkViewModel
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Display(ResourceType = typeof(Properties.Resources), Name = "UserLinkViewModel_Type_Link_Type")] 
        public string Type { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Display(ResourceType = typeof(Properties.Resources), Name = "UserLinkViewModel_Text_Link_Text")] 
        public string Text { get; set; }

        [Url(ErrorMessageResourceName = "ValidUrl", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Display(ResourceType = typeof(Properties.Resources), Name = "UserLinkViewModel_Link_Link_Url")] 
        public string Link { get; set; }

        
    }
}