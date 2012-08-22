using System;
using PostaFlya.Application.Domain.Content;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Query;
using WebSite.Application.Content;

namespace PostaFlya.Models.Browser
{
    public static class BrowserModelBrowserInterfaceExtension
    {
        //TODO use siteService to get default avatar url
        private const string DefaultAvatar = "/Content/themes/taskflya/images/defaultavatar.jpg";

        public static BrowserModel GetBrowserViewModel(string browserId, BrowserQueryServiceInterface browserQuery, BlobStorageInterface blobStorage)
        {
            var browser = browserQuery.FindById<Domain.Browser.Browser>(browserId);
            if (browser == null)
                return null;
            return browser.ToViewModel(blobStorage);
        }

        public static BrowserModel FillViewModel(this BrowserModel browser, BrowserQueryServiceInterface browserQuery, BlobStorageInterface blobStorage)
        {
            var browserDomain = browserQuery.FindById<Domain.Browser.Browser>(browser.Id);
            if (browserDomain == null)
                return null;
            browser.SetBrowserViewModel(browserDomain, blobStorage);
            return browser;
        }

        public static BrowserModel ToViewModel(this BrowserInterface browser, BlobStorageInterface blobStorage)
        {
            var ret = new BrowserModel();
            ret.SetBrowserViewModel(browser, blobStorage);
            return ret;
        }

        public static void SetBrowserViewModel(this BrowserModel view, BrowserInterface domain, BlobStorageInterface blobStorage)
        {
            view.Id = domain.Id;
            view.Handle = domain.Handle;
            view.DisplayName = domain.Handle;

            Uri uri;
            if (string.IsNullOrWhiteSpace(domain.AvatarImageId)
                || (uri = blobStorage.GetBlobUri(domain.AvatarImageId)) == null)
            {
                view.AvatarUrl = DefaultAvatar;
            }
            else
            {
                view.AvatarUrl = uri.GetUrlForImage(ThumbOrientation.Square, ThumbSize.S50);

            }
        }

        public static ModelType FillBrowserModel<ModelType>(this ModelType model, BrowserQueryServiceInterface browserQuery
                , BlobStorageInterface blobStorage) where ModelType : HasBrowserModelInterface
        {
            model.Browser = model.Browser.FillViewModel(browserQuery, blobStorage);
            return model;
        }
 
    }
    public interface HasBrowserModelInterface
    {
        BrowserModel Browser { get; set; }
    }

    public class BrowserModel : ViewModelBase
    {
        public string DisplayName { get; set; }
        public string Id { get; set; }
        public string Handle { get; set; }
        public string AvatarUrl { get; set; }
    }
}