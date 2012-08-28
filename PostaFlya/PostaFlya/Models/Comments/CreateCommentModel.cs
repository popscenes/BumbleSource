using System.ComponentModel;
using Website.Application.Extension.Validation;
using PostaFlya.Binding;
using Website.Domain.Browser;

namespace PostaFlya.Models.Comments
{
    public class CreateCommentModel : BrowserIdInterface
    {
        [RequiredWithMessage]
        public EntityTypeEnum CommentEntity { get; set; }
        
        [DisplayName("Comment")]
        [StringLengthWithMessage(100, MinimumLength = 1)]
        public string Comment { get; set; }     
        
        [RequiredWithMessage]
        public string EntityId { get; set; }
        
        [RequiredWithMessage]
        public string BrowserId { get; set; }
    }
}