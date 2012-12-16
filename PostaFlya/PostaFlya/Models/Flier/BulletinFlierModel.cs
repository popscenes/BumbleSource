﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
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
            var uri = blobStorage.GetBlobUri(model.FlierImageId + ImageUtil.GetIdFileExtension());
            if(uri != null)
                model.FlierImageUrl = uri.ToString();
            return model;
        }

        public static BulletinFlierModel GetImageUrl(this BulletinFlierModel model, BlobStorageInterface blobStorage, ThumbOrientation orientation, ThumbSize thumbSize = ThumbSize.S450)
        {
            var uri = blobStorage.GetBlobUri(model.FlierImageId + ImageUtil.GetIdFileExtension());
            if (uri != null)
                model.FlierImageUrl = uri.GetThumbUrlForImage(orientation, thumbSize);
            return model;
        }
    }

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