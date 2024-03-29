using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Website.Application.Extension.Validation;
using Website.Common.Model;

namespace PostaFlya.Models.Tags
{
    public class AddTagsModel : IsModelInterface
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Display(Name = "AddNewTag", ResourceType = typeof(Properties.Resources))] 
        public string TagsString { get; set; }

        public Website.Domain.Tag.Tags Tags
        {
            get
            {
                return new Website.Domain.Tag.Tags(TagsString);
            }
        }
    }
}