using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Website.Application.Extension.Validation;
using PostaFlya.Binding;
using Website.Domain.Browser;

namespace PostaFlya.Models.Comments
{
    [DataContract]
    public class CreateCommentModel : BrowserIdInterface
    {
        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [DataMember(IsRequired = true)]
        public EntityTypeEnum CommentEntity { get; set; }
        
        [Display(Name = "Comment", ResourceType = typeof(Properties.Resources))]
        [StringLength(100, MinimumLength = 1, ErrorMessageResourceName = "StringTooLargeOrSmall", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [DataMember]
        public string Comment { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [DataMember]
        public string EntityId { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [DataMember]
        public string BrowserId { get; set; }
    }
}