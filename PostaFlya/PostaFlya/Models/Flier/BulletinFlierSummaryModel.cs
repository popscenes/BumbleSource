using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Location;
using PostaFlya.Models.Content;
using Website.Common.Model;
using Website.Common.Model.Query;
using Website.Domain.Browser;
using Website.Infrastructure.Query;

namespace PostaFlya.Models.Flier
{
//    public static class BulletinFlierModelUtil
//    {
//
//        public static IList<BulletinFlierModel> IdsToModel(IEnumerable<string> flierIds, GenericQueryServiceInterface flierQueryService
//            , BlobStorageInterface blobStorage, FlierBehaviourViewModelFactoryInterface viewModelFactory)
//        {
//            var ret = flierQueryService.FindByIds<Domain.Flier.Flier>(flierIds)
//                .ToViewModel(flierQueryService, blobStorage, viewModelFactory);
//            return ret;
//        }
//
//        public static IList<BulletinFlierModel> ToViewModel<FlierType>(this IEnumerable<FlierType> fliers, GenericQueryServiceInterface flierQueryService
//            , BlobStorageInterface blobStorage, FlierBehaviourViewModelFactoryInterface viewModelFactory)
//            where FlierType : FlierInterface
//        {
//            var ret = fliers
//                .Where(f => f != null)
//                .Select(f => viewModelFactory
//                    .GetBulletinViewModel(f, false)
//                    .GetImageUrl(blobStorage))
//                .ToList();
//            return ret;
//        }
//    }
//    public static class BulletinFlierModelFlierInterfaceExtension
//    {
//        public static BulletinFlierModel GetImageUrl(this BulletinFlierModel model, BlobStorageInterface blobStorage)
//        {
//            var uri = blobStorage.GetBlobUri(model.FlierImageId + ImageUtil.GetIdFileExtension());
//            if(uri != null)
//                model.FlierImageUrl = uri.ToString();
//            return model;
//        }
//
//        public static BulletinFlierModel GetImageUrl(this BulletinFlierModel model, BlobStorageInterface blobStorage, ThumbOrientation orientation, ThumbSize thumbSize = ThumbSize.S456)
//        {
//            var uri = blobStorage.GetBlobUri(model.FlierImageId + ImageUtil.GetIdFileExtension());
//            if (uri != null)
//                model.FlierImageUrl = uri.GetThumbUrlForImage(orientation, thumbSize);
//            return model;
//        }
//    }

    public class ToBulletinFlierModel : ViewModelMapperInterface<BulletinFlierSummaryModel, PostaFlya.Domain.Flier.Flier>
    {
        private readonly QueryChannelInterface _queryChannel;

        public ToBulletinFlierModel(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public BulletinFlierSummaryModel ToViewModel(BulletinFlierSummaryModel target, Domain.Flier.Flier flier)
        {
            if(target == null)
                target = new BulletinFlierSummaryModel();

            target.Id = flier.Id;
            target.FriendlyId = flier.FriendlyId;
            target.Title = flier.Title;
            target.Venue = _queryChannel.ToViewModel<VenueInformationModel>(flier.Venue);
            target.EventDates = flier.EventDates;
            target.CreateDate = flier.CreateDate;
            target.TagsString = flier.Tags.ToString();
            target.FlierImageId = flier.Image.HasValue ? flier.Image.Value.ToString() : null;
            target.NumberOfClaims = flier.NumberOfClaims;
            target.NumberOfComments = flier.NumberOfComments;
            target.BrowserId = flier.BrowserId;
            target.ImageList = _queryChannel.ToViewModel<ImageViewModel, FlierImage>(flier.ImageList);
            target.PendingCredits = flier.Features.Sum(_ => _.OutstandingBalance);
            target.Status = flier.Status.ToString();
            target.TinyUrl = flier.TinyUrl;


            return target;
        }
    }


    public class BulletinFlierSummaryModel : BrowserIdInterface
    {
        
        [Display(Name = "FlierId", ResourceType = typeof(Properties.Resources))] 
        public string Id { get; set; }

        [Display(Name = "FriendlyId", ResourceType = typeof(Properties.Resources))] 
        public string FriendlyId { get; set; }

        [Display(Name = "FlierTitle", ResourceType = typeof(Properties.Resources))] 
        public string Title { get; set; }

        [Display(Name = "FlierLocation", ResourceType = typeof(Properties.Resources))]
        public VenueInformationModel Venue { get; set; }

        [Display(Name = "FlierTags", ResourceType = typeof(Properties.Resources))] 
        public string TagsString { get; set; }

        [Display(Name = "EffectiveDate", ResourceType = typeof(Properties.Resources))] 
        public List<DateTimeOffset> EventDates { get; set; }

        [Display(Name = "FlierImage", ResourceType = typeof(Properties.Resources))] 
        public string FlierImageUrl { get; set; }

        public string FlierImageId { get; set; }


        [Display(Name = "NumberOfClaims", ResourceType = typeof(Properties.Resources))] 
        public int NumberOfClaims { get; set; }

        [Display(Name = "NumberOfComments", ResourceType = typeof(Properties.Resources))] 
        public int NumberOfComments { get; set; }

        public string BrowserId { get; set; }

        [Display(Name = "CreateDate", ResourceType = typeof(Properties.Resources))] 
        public DateTime CreateDate { get; set; }

        [Display(Name = "FlierImages", ResourceType = typeof(Properties.Resources))] 
        public List<ImageViewModel> ImageList { get; set; }

        public Int32 PendingCredits { get; set; }

        [Display(Name = "FlierStatus", ResourceType = typeof(Properties.Resources))] 
        public string Status { get; set; }

        public string TinyUrl { get; set; }

        public static BulletinFlierSummaryModel DefaultForTemplate()
        {
            return new BulletinFlierSummaryModel()
                {
                    Venue = new VenueInformationModel() { Address = new LocationModel()}
                };
        }
    }

}