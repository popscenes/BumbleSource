using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
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
                           //Location = flier.Location.ToViewModel(),
                           EventDates = flier.EventDates.Select(d => d.DateTime).ToList(),
                           TagsString = flier.Tags.ToString(),
                           FlierImageId = flier.Image.HasValue? flier.Image.Value.ToString(): "",
                           FlierBehaviour = flier.FlierBehaviour,
                           ImageList = flier.ImageList.Select(_ => new ImageViewModel(){ImageId = _.ImageID}).ToList(),
                           ExternalSource = flier.ExternalSource,
                           ExternalId = flier.ExternalId,
                           BoardList =  flier.Boards != null ? flier.Boards.Select(_ => _.BoardId).ToList() : new List<string>(),
                           EnableAnalytics =  flier.EnableAnalytics,
                           //PostRadius = flier.LocationRadius+5,
                           //VenueInformation = flier.Venue.ToViewModel(),
                           //TotalPaid = flier.GetTotalPaid(),
                           UserLinks = flier.UserLinks == null ? new List<UserLinkViewModel>() : flier.UserLinks.Select(_ => _.ToViewModel()).ToList()
                       };
        }

        public static FlierCreateModel GetImageUrl(this FlierCreateModel model, BlobStorageInterface blobStorage)
        {
            var uri = blobStorage.GetBlobUri(model.FlierImageId + ImageUtil.GetIdFileExtension());
            if (uri != null)
                model.FlierImageUrl = uri.ToString();
            return model;
        }

        public static FlierCreateModel GetImageUrl(this FlierCreateModel model, BlobStorageInterface blobStorage, ThumbOrientation orientation, ThumbSize thumbSize)
        {
            var uri = blobStorage.GetBlobUri(model.FlierImageId + ImageUtil.GetIdFileExtension());
            if (uri != null)
                model.FlierImageUrl = uri.GetThumbUrlForImage(orientation, thumbSize);
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
            UserLinks = new List<UserLinkViewModel>();
        }
        [Display(Name = "FlierId", ResourceType = typeof(Properties.Resources))] 
        [DataMember]
        public string Id { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Display(Name = "FlierTitle", ResourceType = typeof(Properties.Resources))] 
        [DataMember]
        public string Title { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [StringLength(2000, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Display(Name = "FlierDescription", ResourceType = typeof(Properties.Resources))] //TODO change to LocalizedDisplayName
        [DataMember]
        public string Description { get; set; }

//        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
//        [ValidLocation]
//        [Display(Name = "FlierLocation", ResourceType = typeof(Properties.Resources))] 
//        [DataMember]
//        public LocationModel Location { get; set; }
//
//        [Display(Name = "FlierCreateModel_PostRadius", ResourceType = typeof(Properties.Resources))]
//        [DataMember(IsRequired = false)]
//        public int PostRadius { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [StringLength(100, ErrorMessageResourceName = "StringTooLarge", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Display(Name = "FlierTags", ResourceType = typeof(Properties.Resources))] 
        [DataMember]
        public string TagsString { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [ConvertableToGuid(ErrorMessageResourceName = "InvalidGuid", ErrorMessageResourceType = typeof(Properties.Resources), ErrorMessage = null)]
        [Display(Name = "FlierImage", ResourceType = typeof(Properties.Resources))] 
        [DataMember]
        public String FlierImageId { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Display(Name = "BehaviourType", ResourceType = typeof(Properties.Resources))] 
        [DataMember(IsRequired = true)]
        public FlierBehaviour FlierBehaviour { get; set; }

        [Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Display(Name = "EffectiveDate", ResourceType = typeof(Properties.Resources))] 
        [DataMember(IsRequired = true)]
        public List<DateTime> EventDates { get; set; }

        [Display(Name = "FlierImages", ResourceType = typeof(Properties.Resources))] 
        [DataMember]
        public List<ImageViewModel> ImageList { get; set; }

        [Display(Name = "FlierImage", ResourceType = typeof(Properties.Resources))] 
        [DataMember]
        public String FlierImageUrl { get; set; }

        //[Display(Name = "AttachContactDetails", ResourceType = typeof(Properties.Resources))] 
        //[DataMember]
        //public bool AttachContactDetails { get; set; }

        [DataMember]
        public String ExternalSource { get; set; }

        [DataMember]
        public String ExternalId { get; set; }

        [Display(Name = "BoardList", ResourceType = typeof(Properties.Resources))] 
        [DataMember]
        public List<string> BoardList { get; set; }

        [DataMember]
        public bool AllowUserContact { get; set; }

        [DataMember]
        [Display(Name = "FlierCreateModel_EnableAnalytics_EnableAnalytics", ResourceType = typeof(Properties.Resources))] 
        public bool EnableAnalytics { get; set; }

        [DataMember(IsRequired = false)]
        public bool Anonymous { get; set; }

        [DataMember]
        public List<UserLinkViewModel> UserLinks {get; set; }

        public static FlierCreateModel DefaultForTemplate()
        {
            return new FlierCreateModel();
        }
    }
}