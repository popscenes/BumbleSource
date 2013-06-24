using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Location;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Domain.Content;
using Website.Common.Model;
using Website.Common.Model.Query;
using Website.Infrastructure.Query;
using Res = PostaFlya.Properties.Resources;

namespace PostaFlya.Areas.MobileApi.Flyers.Model
{
    public class ToFlyerSummaryModel : ViewModelMapperInterface<FlyerSummaryModel, Flier>
    {
        private readonly BlobStorageInterface _blobStorage;
        private readonly QueryChannelInterface _queryChannel;

        public ToFlyerSummaryModel([ImageStorage]BlobStorageInterface blobStorage, QueryChannelInterface queryChannel)
        {
            _blobStorage = blobStorage;
            _queryChannel = queryChannel;
        }

        public FlyerSummaryModel ToViewModel(FlyerSummaryModel target, Flier source)
        {
            if(target == null)
                target = new FlyerSummaryModel();

            target.Id = source.Id;
            target.FriendlyId = source.FriendlyId;
            target.ImageUrl = _blobStorage.GetBlobUri(source.Image.ToString() + ImageUtil.GetIdFileExtension()).ToString();
            target.Title = source.Title;
            target.EventDates = source.EventDates;
            
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

        [Display(Name = "Flyer_Venue", ResourceType = typeof (Res))]
        public VenueInformationModel Venue{ get; set; }
    }
}