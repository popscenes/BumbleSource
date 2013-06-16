using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PostaFlya.Domain.Flier;
using Website.Common.Model;
using Res = PostaFlya.Properties.Resources;

namespace PostaFlya.Areas.MobileApi.Flyers.Model
{
    public class ToFlyerSummaryModel : ViewModelMapperInterface<FlyerSummaryModel, Flier>
    {
        public FlyerSummaryModel ToViewModel(FlyerSummaryModel target, Flier source)
        {
            if(target == null)
                target = new FlyerSummaryModel();
            
            return target;

        }
    }
    public class FlyerSummaryModel
    {

        [Display(Name = "Id", ResourceType = typeof(Res))]
        public string Id { get; set; }

        [Display(Name = "FriendlyId", ResourceType = typeof(Res))]
        public string FriendlyId { get; set; }

        [Display(Name = "Flyer_EventDates", ResourceType = typeof(Res))]
        public List<DateTimeOffset> EventDates { get; set; }

        [Display(Name = "Flyer_ImageUrl", ResourceType = typeof (Res))]
        public string ImageUrl { get; set; }

        [Display(Name = "Flyer_Title", ResourceType = typeof (Res))]
        public string Title { get; set; }
    }
}