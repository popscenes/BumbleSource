using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Website.Application.Content;
using Website.Application.Extension.Validation;
using PostaFlya.Binding;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Domain;
using PostaFlya.Models.Browser;
using Website.Domain.Comments;
using Website.Infrastructure.Query;

namespace PostaFlya.Models.Comments
{
    public static class CommentModelCommentInterfaceExtension
    {
        public static CommentModel ToViewModel(this CommentInterface comment
            , GenericQueryServiceInterface browserQuery
            , BlobStorageInterface blobStorage)
        {
            var ret = comment.ToViewModel();
            return ret.FillBrowserModel(browserQuery, blobStorage);
        }

        public static CommentModel ToViewModel(this CommentInterface comment)
        {
            var ret = new CommentModel()
            {
                Browser = new BrowserModel() { Id = comment.BrowserId },
                Comment = comment.CommentContent,
                CommentTime = comment.CommentTime,
            };

            return ret;
        }

    }

    public class CommentModel : HasBrowserModelInterface
    {        
        public EntityTypeEnum CommentEntity { get; set; }       
        public string EntityId { get; set; }

        [Display(Name = "Comment", ResourceType = typeof(Properties.Resources))] 
        public string Comment { get; set; }

        [Display(Name = "CommentTime", ResourceType = typeof(Properties.Resources))] 
        public DateTime CommentTime { get; set; }

        public BrowserModel Browser{ get; set; }

        public static CommentModel DefaultForTemplate()
        {
            return new CommentModel()
            {
                Browser = new BrowserModel()
            };
        }
    }
}