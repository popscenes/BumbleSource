using System;
using System.Collections.Generic;
using System.ComponentModel;
using WebSite.Application.Content;
using WebSite.Application.Extension.Validation;
using PostaFlya.Binding;
using WebSite.Infrastructure.Authentication;
using WebSite.Infrastructure.Domain;
using PostaFlya.Models.Browser;
using Website.Domain.Browser.Query;
using Website.Domain.Comments;

namespace PostaFlya.Models.Comments
{
    public static class CommentModelCommentInterfaceExtension
    {
        public static CommentModel ToViewModel(this CommentInterface comment
            , BrowserQueryServiceInterface browserQuery
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

        [DisplayName("Comment")]
        public string Comment { get; set; }

        [DisplayName("CommentTime")]
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