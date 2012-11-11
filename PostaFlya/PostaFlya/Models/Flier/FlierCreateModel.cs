using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using Website.Application.Content;
using Website.Application.Extension.Validation;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Models.Location;
using System.Collections.Generic;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Content;
using Website.Application.Domain.Content;
using Website.Application.Domain.Location;
using System.Runtime.Serialization;

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
                           FlierImageId = flier.Image.HasValue? flier.Image.Value.ToString(): "",
                           FlierBehaviour = flier.FlierBehaviour,
                           ImageList = flier.ImageList.Select(_ => new ImageViewModel(){ImageId = _.ImageID}).ToList(),
                           ExternalSource = flier.ExternalSource,
                           ExternalId = flier.ExternalId,
                           AttachContactDetails = flier.HasContactDetails(),
                           BoardList =  flier.Boards ?? new List<string>(),
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

    [DataContract]
    public class FlierCreateModel : ViewModelBase
    {

        public FlierCreateModel()
        {
            ImageList = new List<ImageViewModel>();
            BoardList = new List<string>();
        }
        [DisplayName("FlierId")]
        [DataMember]
        public string Id { get; set; }

        [RequiredWithMessage]
        [StringLengthWithMessage(100)]
        [DisplayName("FlierTitle")]
        [DataMember]
        public string Title { get; set; }

        [RequiredWithMessage]
        [StringLengthWithMessage(2000)]
        [DisplayName("FlierDescription")]//TODO change to LocalizedDisplayName
        [DataMember]
        public string Description { get; set; }

        [RequiredWithMessage]
        [ValidLocation]
        [DisplayName("FlierLocation")]
        [DataMember]
        public LocationModel Location { get; set; }

        [RequiredWithMessage]
        [StringLengthWithMessage(100)]
        [DisplayName("FlierTags")]
        [DataMember]
        public string TagsString { get; set; }

        [RequiredWithMessage]
        [ConvertableToGuidAttributeWithMessage]
        [DisplayName("FlierImage")]
        [DataMember]
        public String FlierImageId { get; set; }

        [RequiredWithMessage]
        [DisplayName("FlierType")]
        [DataMember(IsRequired = true)]
        public FlierBehaviour FlierBehaviour { get; set; }

        [RequiredWithMessage]
        [DisplayName("FlierDate")]
        [DataMember(IsRequired = true)]
        public DateTime EffectiveDate { get; set; }

        [DisplayName("ImageList")]
        [DataMember]
        public List<ImageViewModel> ImageList { get; set; }

        [DisplayName("FlierImageUrl")]
        [DataMember]
        public String FlierImageUrl { get; set; }

        [DisplayName("AttachContactDetails")]
        [DataMember]
        public bool AttachContactDetails { get; set; }

        [DataMember]
        public String ExternalSource { get; set; }

        [DataMember]
        public String ExternalId { get; set; }

        [DisplayName("BoardList")]
        [DataMember]
        public List<string> BoardList { get; set; }
       
        public static FlierCreateModel DefaultForTemplate()
        {
            return new FlierCreateModel();
        }
    }
}