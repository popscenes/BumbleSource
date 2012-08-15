using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using WebSite.Application.Extension.Validation;

namespace PostaFlya.Models.Tags
{
    public class AddTagsModel : ViewModelBase
    {
        [RequiredWithMessage]
        [StringLengthWithMessage(100)]
        [DisplayName("AddNewTag")]
        public string TagsString { get; set; }

        public Domain.Tag.Tags Tags
        {
            get
            {
                return new Domain.Tag.Tags(TagsString);
            }
        }
    }
}