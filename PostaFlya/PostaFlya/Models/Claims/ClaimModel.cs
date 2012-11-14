using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PostaFlya.Binding;
using PostaFlya.Models.Browser;
using Website.Application.Content;
using Website.Domain.Browser.Query;
using Website.Domain.Claims;

namespace PostaFlya.Models.Claims
{
    public static class ClaimModelInterfaceExtension
    {
        public static ClaimModel ToViewModel(this ClaimInterface claim
            , BrowserQueryServiceInterface browserQuery
            , BlobStorageInterface blobStorage)
        {
            var ret = claim.ToViewModel();
            return ret.FillBrowserModel(browserQuery, blobStorage);
        }

        public static ClaimModel ToViewModel(this ClaimInterface claim)
        {
            return new ClaimModel()
            {
                Browser = new BrowserModel(){Id = claim.BrowserId},
                ClaimTime = claim.ClaimTime
            };
        }
    }

    public class ClaimModel : HasBrowserModelInterface
    {
        public EntityTypeEnum ClaimEntity { get; set; }
        public string EntityId { get; set; }

        [Display(Name = "ClaimTime", ResourceType = typeof(Properties.Resources))] 
        public DateTime ClaimTime { get; set; }

        public BrowserModel Browser { get; set; }

        public static ClaimModel DefaultForTemplate()
        {
            return new ClaimModel()
                       {
                           Browser = new BrowserModel()
                       };
        }
    }
}