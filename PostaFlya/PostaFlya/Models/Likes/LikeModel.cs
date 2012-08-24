using System;
using System.ComponentModel;
using PostaFlya.Binding;
using PostaFlya.Models.Browser;
using WebSite.Application.Content;
using Website.Domain.Browser.Query;
using Website.Domain.Likes;

namespace PostaFlya.Models.Likes
{
    public static class LikeModelLikeInterfaceExtension
    {
        public static LikeModel ToViewModel(this LikeInterface like
            , BrowserQueryServiceInterface browserQuery
            , BlobStorageInterface blobStorage)
        {
            var ret = like.ToViewModel();
            return ret.FillBrowserModel(browserQuery, blobStorage);
        }

        public static LikeModel ToViewModel(this LikeInterface like)
        {
            return new LikeModel()
            {
                Browser = new BrowserModel(){Id = like.BrowserId},
                LikeTime = like.LikeTime
            };
        }
    }

    public class LikeModel : HasBrowserModelInterface
    {
        public EntityTypeEnum LikeEntity { get; set; }
        public string EntityId { get; set; }

        [DisplayName("LikeTime")]
        public DateTime LikeTime { get; set; }

        public BrowserModel Browser { get; set; }

        public static LikeModel DefaultForTemplate()
        {
            return new LikeModel()
                       {
                           Browser = new BrowserModel()
                       };
        }
    }
}