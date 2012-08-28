using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using Website.Application.Content;
using Website.Application.Domain.Browser;

namespace PostaFlya.Models.Browser
{
    public class CurrentBrowserModel : BrowserModel
    {
        public String BrowserId { get; set; }
        public List<LocationModel> SavedLocations { get; set; }
        public List<String> SavedTags { get; set; }
        public List<string> Roles { get; set; }
    }

    public static class BulletinFlierModelFlierInterfaceExtension
    {
        public static CurrentBrowserModel ToCurrentBrowserModel(this BrowserInformationInterface browserInfo, BlobStorageInterface blobStorage)
        {
            var ret =  new CurrentBrowserModel()
            {
                Handle = browserInfo.Browser.Handle,
                BrowserId = browserInfo.Browser.Id,
                SavedLocations = browserInfo.Browser.SavedLocations.Select(_ => _.ToViewModel()).ToList(),
                SavedTags = browserInfo.Browser.SavedTags.Select(_ => _.ToString()).ToList(),
                Roles = browserInfo.Browser.Roles.Select(r => r).ToList()
            };

            ret.SetBrowserViewModel(browserInfo.Browser, blobStorage);
            return ret;
        }
    }
}