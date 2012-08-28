using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using Website.Application.Content;
using Website.Application.Extension.Validation;
using PostaFlya.Areas.Default.Models;
using PostaFlya.Areas.Default.Models.Bulletin;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;
using PostaFlya.Areas.TaskJob.Models;
using PostaFlya.Models.Location;
using PostaFlya.Models.Content;
using Website.Application.Domain.Content;

namespace PostaFlya.Models.Flier
{
    public static class BulletinFlierModelFlierInterfaceExtension
    {
//        public static BulletinFlierModel ToViewModel(this FlierInterface flier, bool detailMode)
//        {
//            //dynamic behav = flier.Behaviour;
//            return new BulletinFlierModel()
//                       {
//                           Id = flier.Id,
//                           Title = flier.Title,
//                           Description = (!detailMode && !string.IsNullOrWhiteSpace(flier.Description)) ?  (flier.Description.Length > 200 ? flier.Description.Substring(0, 200) : flier.Description) : flier.Description,
//                           Location = flier.Location.ToViewModel(),
//                           EffectiveDate = flier.EffectiveDate,
//                           CreateDate = flier.CreateDate,
//                           TagsString = flier.Tags.ToString(),
//                           FlierImageId = flier.Image.HasValue ? flier.Image.Value.ToString() : null,
//                           FlierBehaviour = flier.FlierBehaviour.ToString(),   
//                           //Behaviour = flier.ToBehaviourSummaryViewModel(),
//                           NumberOfClaims = flier.NumberOfClaims,
//                           NumberOfComments = flier.NumberOfComments,
//                           BrowserId = flier.BrowserId,
//                           ImageList = flier.ImageList.Select(_ => new ImageViewModel() { ImageId = _.ImageID }).ToList()
//
//                       };
//        }


        public static BulletinFlierModel GetImageUrl(this BulletinFlierModel model, BlobStorageInterface blobStorage)
        {
            var uri = blobStorage.GetBlobUri(model.FlierImageId);
            if(uri == null) return model;
            model.FlierImageUrl = uri.ToString();
            return model;
        }

        public static BulletinFlierModel GetDefaultImageUrl(this BulletinFlierModel model, BlobStorageInterface blobStorage, ThumbOrientation orientation = ThumbOrientation.Horizontal, ThumbSize thumbSize = ThumbSize.S450)
        {
            var uri = blobStorage.GetBlobUri(model.FlierImageId);
            if (uri == null) return model;
            model.FlierImageUrl = uri.GetUrlForImage(orientation, thumbSize);
            return model;
        }
    }

    public class BulletinFlierModel
    {
        
        [DisplayName("FlierId")]//TODO change to LocalizedDisplayName
        public string Id { get; set; }

        [DisplayName("FlierTitle")]
        public string Title { get; set; }

        [DisplayName("FlierDescription")]
        public string Description { get; set; }

        [DisplayName("FlierLocation")]
        public LocationModel Location { get; set; }

        [DisplayName("FlierTags")]
        public string TagsString { get; set; }

        [DisplayName("EffectiveDate")]
        public DateTime EffectiveDate { get; set; }

        [DisplayName("FlierImage")]
        public string FlierImageUrl { get; set; }

        public string FlierImageId { get; set; }

        [DisplayName("BehaviourType")]
        public string FlierBehaviour { get; set; }

        [DisplayName("NumberOfClaims")]
        public int NumberOfClaims { get; set; }

        [DisplayName("NumberOfComments")]
        public int NumberOfComments { get; set; }

        public string BrowserId { get; set; }

        [DisplayName("CreateDate")]
        public DateTime CreateDate { get; set; }

        public List<ImageViewModel> ImageList { get; set; }
    }

    public class BulletinFlierModel<BehaviourType> : BulletinFlierModel where BehaviourType : new()
    {
        public BehaviourType Behaviour { get; set; }

        public static BulletinFlierModel DefaultForTemplate(FlierBehaviour behaviour)
        {
            var flier = new Domain.Flier.Flier() { FlierBehaviour = behaviour, Location = new Website.Domain.Location.Location() };
            var ret = flier.ToViewModel<BehaviourType>(false);
            ret.Behaviour = new BehaviourType();
            ret.ImageList = new List<ImageViewModel>() { new ImageViewModel() };
            return ret;
        }
    }
}