using System.ComponentModel;
using WebSite.Application.Extension.Validation;
using PostaFlya.Binding;

namespace PostaFlya.Models.Likes
{
    public class CreateLikeModel
    {
        [RequiredWithMessage]
        public EntityTypeEnum LikeEntity { get; set; }
        
        [DisplayName("Comment")]
        [StringLengthWithMessage(100)]
        public string Comment { get; set; }     
        
        [RequiredWithMessage]
        public string EntityId { get; set; }
        
        [RequiredWithMessage]
        public string BrowserId { get; set; }
    }
}