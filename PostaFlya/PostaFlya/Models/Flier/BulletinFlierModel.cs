using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Newtonsoft.Json;
using PostaFlya.Areas.TaskJob.Models.Bulletin;
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
using Website.Domain.Browser;
using Website.Infrastructure.Query;

namespace PostaFlya.Models.Flier
{
    public static class BulletinFlierModelFlierInterfaceExtension
    {
        public static BulletinFlierModel GetImageUrl(this BulletinFlierModel model, BlobStorageInterface blobStorage)
        {
            var uri = blobStorage.GetBlobUri(model.FlierImageId + ImageUtil.GetIdFileExtension());
            if(uri != null)
                model.FlierImageUrl = uri.ToString();
            return model;
        }

        public static BulletinFlierModel GetImageUrl(this BulletinFlierModel model, BlobStorageInterface blobStorage, ThumbOrientation orientation, ThumbSize thumbSize = ThumbSize.S456)
        {
            var uri = blobStorage.GetBlobUri(model.FlierImageId + ImageUtil.GetIdFileExtension());
            if (uri != null)
                model.FlierImageUrl = uri.GetThumbUrlForImage(orientation, thumbSize);
            return model;
        }
    }

    [KnownType(typeof(BulletinFlierModel<BulletinBehaviourModel>))]
    [KnownType(typeof(BulletinFlierModel<BulletinTaskJobBehaviourModel>))]
    public class BulletinFlierModel
    {
        
        [Display(Name = "FlierId", ResourceType = typeof(Properties.Resources))] 
        public string Id { get; set; }

        [Display(Name = "FriendlyId", ResourceType = typeof(Properties.Resources))] 
        public string FriendlyId { get; set; }

        [Display(Name = "FlierTitle", ResourceType = typeof(Properties.Resources))] 
        public string Title { get; set; }

        [Display(Name = "FlierDescription", ResourceType = typeof(Properties.Resources))] 
        public string Description { get; set; }

        [Display(Name = "FlierLocation", ResourceType = typeof(Properties.Resources))] 
        public LocationModel Location { get; set; }

        [Display(Name = "FlierTags", ResourceType = typeof(Properties.Resources))] 
        public string TagsString { get; set; }

        [Display(Name = "EffectiveDate", ResourceType = typeof(Properties.Resources))] 
        public DateTime EffectiveDate { get; set; }

        [Display(Name = "FlierImage", ResourceType = typeof(Properties.Resources))] 
        public string FlierImageUrl { get; set; }

        public string FlierImageId { get; set; }

        [Display(Name = "BehaviourType", ResourceType = typeof(Properties.Resources))] 
        public string FlierBehaviour { get; set; }

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