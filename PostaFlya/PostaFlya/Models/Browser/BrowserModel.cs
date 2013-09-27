using System;
using System.Runtime.Serialization;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Domain.Content;
using Website.Common.Model;
using Website.Domain.Browser;
using Website.Domain.Content;
using Website.Infrastructure.Query;

namespace PostaFlya.Models.Browser
{
    public static class BrowserModelBrowserInterfaceExtension
    {
//        //TODO use siteService to get default avatar url
//        //private const string DefaultAvatar = "/Content/themes/taskflya/images/icon_0001_Group-5.png";
//
//        public static BrowserModel GetBrowserViewModel(string browserId, GenericQueryServiceInterface browserQuery, BlobStorageInterface blobStorage)
//        {
//            var browser = browserQuery.FindById<Website.Domain.Browser.Browser>(browserId);
//            if (browser == null)
//                return null;
//            return browser.ToViewModel(blobStorage);
//        }
//
//        public static BrowserModel FillViewModel(this BrowserModel browser, GenericQueryServiceInterface browserQuery, BlobStorageInterface blobStorage)
//        {
//            var browserDomain = browserQuery.FindById<Website.Domain.Browser.Browser>(browser.Id);
//            if (browserDomain == null)
//                return null;
//            browser.SetBrowserViewModel(browserDomain, blobStorage);
//            return browser;
//        }
//
//
//
//        public static ModelType FillBrowserModel<ModelType>(this ModelType model, GenericQueryServiceInterface browserQuery
//                , BlobStorageInterface blobStorage) where ModelType : HasBrowserModelInterface
//        {
//            model.Browser = model.Browser.FillViewModel(browserQuery, blobStorage);
//            return model;
//        }
 
    }
    public interface HasBrowserModelInterface
    {
        BrowserModel Browser { get; set; }
    }

    public class ToBrowserViewModel : ViewModelMapperInterface<BrowserModel, PostaFlya.Domain.Browser.Browser>
    {
        private readonly BlobStorageInterface _blobStorage;

        public ToBrowserViewModel([ImageStorage]BlobStorageInterface blobStorage)
        {
            _blobStorage = blobStorage;
        }

        public BrowserModel ToViewModel(BrowserModel target, PostaFlya.Domain.Browser.Browser source)
        {
            if(target == null)
                target = new BrowserModel();
            
            target.Id = source.Id;
            target.Handle = source.FriendlyId;
            target.DisplayName = source.FriendlyId;

            Uri uri;
            if (!string.IsNullOrWhiteSpace(source.AvatarImageId)
                && (uri = _blobStorage.GetBlobUri(source.AvatarImageId + ImageUtil.GetIdFileExtension())) != null)
            {
                target.AvatarUrl = uri.GetThumbUrlForImage(ThumbOrientation.Square, ThumbSize.S300);

            }

            return target;
        }
    }

    [Serializable]
    [DataContract]
    public class BrowserModel : IsModelInterface
    {
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Handle { get; set; }
        [DataMember]
        public string AvatarUrl { get; set; }
    }
}