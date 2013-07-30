using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Common.Model;
using Website.Domain.Content;
using Website.Infrastructure.Query;
using Res = PostaFlya.Properties.Resources;

namespace PostaFlya.Areas.WebApi.Flyers.Model
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
            target.Image = _queryChannel.Query(new FindByIdQuery<Image>() {Id = source.Image.ToString()}, new ImageModel());
            target.Title = source.Title;
            target.EventDates = source.EventDates;
            target.VenueBoard = _queryChannel.Query(new GetFlyerVenueBoardQuery() {FlyerId = source.Id},
                                                    new FlyerBoardSummaryModel());
            
            return target;

        }
    }

    public class FlyerSummaryModel : IsModelInterface
    {

        [Display(Name = "Id", ResourceType = typeof(Res))]
        public string Id { get; set; }

        [Display(Name = "FriendlyId", ResourceType = typeof(Res))]
        public string FriendlyId { get; set; }

        [Display(Name = "Flyer_EventDates", ResourceType = typeof(Res))]
        public List<DateTimeOffset> EventDates { get; set; }

        [Display(Name = "Flyer_ImageUrl", ResourceType = typeof (Res))]
        public ImageModel Image { get; set; }

        [Display(Name = "Flyer_Title", ResourceType = typeof (Res))]
        public string Title { get; set; }

        [Display(Name = "Flyer_Venue_Board", ResourceType = typeof(Res))]
        public FlyerBoardSummaryModel VenueBoard { get; set; }
    }
}