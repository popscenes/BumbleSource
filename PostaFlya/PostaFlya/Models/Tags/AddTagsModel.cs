using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Website.Application.Extension.Validation;

namespace PostaFlya.Models.Tags
{
    public class AddTagsModel : ViewModelBase
    {
        [RequiredWithMessage]
        [StringLengthWithMessage(100)]
        [DisplayName("AddNewTag")]
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