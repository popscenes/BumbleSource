using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using PostaFlya.Binding;
using PostaFlya.Models.Browser;
using Website.Application.Content;
using Website.Domain.Claims;
using Website.Infrastructure.Query;

namespace PostaFlya.Models.Claims
{
    public static class ClaimModelInterfaceExtension
    {

        public static ClaimModel ToViewModel(this ClaimInterface claim)
        {
            var ret = new ClaimModel()
            {
                //Browser = new BrowserModel(){Id = claim.BrowserId},
                ClaimId = claim.Id,
                ClaimTime = claim.ClaimTime,
                EntityId = claim.AggregateId
            };

            EntityTypeEnum val;
            if (Enum.TryParse(claim.AggregateTypeTag, true, out val))
                ret.ClaimEntity = val;
            
            return ret;
        }
    }

    public class ClaimModel
    {
        public EntityTypeEnum ClaimEntity { get; set; }
        public string EntityId { get; set; }
        public string ClaimId { get; set; }

        [Display(Name = "ClaimTime", ResourceType = typeof(Properties.Resources))] 
        public DateTime ClaimTime { get; set; }

        public static ClaimModel DefaultForTemplate()
        {
            return new ClaimModel(){ };
        }
    }
}