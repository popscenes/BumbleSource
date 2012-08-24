using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using WebSite.Application.Content;
using WebSite.Application.Extension.Validation;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Models.Location;
using System.Collections.Generic;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Content;
using Website.Application.Domain.Content;
using Website.Application.Domain.Location;

namespace PostaFlya.Models.Flier
{
    public static class FlierCreateModelExtensions
    {
        public static FlierCreateModel ToCreateModel(this FlierInterface flier)
        {
            //dynamic behav = flier.Behaviour;
            return new FlierCreateModel()
                       {
                           Id = flier.Id,
                           Title = flier.Title,
                           Description = flier.Description,
                           Location = flier.Location.ToViewModel(),
                           EffectiveDate = flier.EffectiveDate,
                           TagsString = flier.Tags.ToString(),
                           FlierImageId = flier.Image,
                           FlierBehaviour = flier.FlierBehaviour,
                           ImageList = flier.ImageList.Select(_ => new ImageViewModel(){ImageId = _.ImageID}).ToList()
                       };
        }

        public static FlierCreateModel GetDefaultImageUrl(this FlierCreateModel model, BlobStorageInterface blobStorage, ThumbOrientation orientation = ThumbOrientation.Horizontal, ThumbSize thumbSize = ThumbSize.S450)
        {
            var uri = blobStorage.GetBlobUri(model.FlierImageId.ToString());
            if (uri == null) return model;
            model.FlierImageUrl = uri.GetUrlForImage(orientation, thumbSize);
            return model;
        }
    }

    public class FlierCreateModel : ViewModelBase
    {

        public FlierCreateModel()
        {
            ImageList = new List<ImageViewModel>();
        }
        [DisplayName("FlierId")]
        public string Id { get; set; }

        [RequiredWithMessage]
        [StringLengthWithMessage(100)]
        [DisplayName("FlierTitle")]
        public string Title { get; set; }

        [RequiredWithMessage]
        [StringLengthWithMessage(2000)]
        [DisplayName("FlierDescription")]//TODO change to LocalizedDisplayName
        public string Description { get; set; }

        [RequiredWithMessage]
        [ValidLocation]
        [DisplayName("FlierLocation")]
        public LocationModel Location { get; set; }

        [RequiredWithMessage]
        [StringLengthWithMessage(100)]
        [DisplayName("FlierTags")]
        public string TagsString { get; set; }

        [RequiredWithMessage]
        [DisplayName("FlierImage")]
        public Guid? FlierImageId { get; set; }

        [RequiredWithMessage]
        [DisplayName("FlierType")]
        public FlierBehaviour FlierBehaviour { get; set; }

        [RequiredWithMessage]
        [DisplayName("FlierDate")]
        public DateTime EffectiveDate { get; set; }

        [DisplayName("ImageList")]
        public List<ImageViewModel> ImageList { get; set; }

        public String FlierImageUrl { get; set; }

        public static FlierCreateModel DefaultForTemplate()
        {
            return new FlierCreateModel();
        }
    }
}