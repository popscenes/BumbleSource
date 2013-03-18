using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using Website.Application.Content;
using Website.Application.Domain.Browser;
using Website.Domain.Contact;

namespace PostaFlya.Models.Browser
{
    public class CurrentBrowserModel : BrowserModel
    {
        public String BrowserId { get; set; }
        public List<LocationModel> SavedLocations { get; set; }
        public List<String> SavedTags { get; set; }
        public List<string> Roles { get; set; }
        public double Credits { get; set; }  
        public ContactDetailsModel ContactDetails { get; set; }
        public LocationModel LastSearchedLocation { get; set; }
    }

    public static class BulletinFlierModelFlierInterfaceExtension
    {
        public static CurrentBrowserModel ToCurrentBrowserModel(this PostaFlyaBrowserInformationInterface browserInfo, BlobStorageInterface blobStorage)
        {
            var ret =  new CurrentBrowserModel()
            {
                Handle = browserInfo.Browser.FriendlyId,
                BrowserId = browserInfo.Browser.Id,
                SavedLocations = browserInfo.Browser.SavedLocations.Select(_ => _.ToViewModel()).ToList(),
                SavedTags = browserInfo.Browser.SavedTags.Select(_ => _.ToString()).ToList(),
                Roles = browserInfo.Browser.Roles.Select(r => r).ToList(),
                Credits = browserInfo.Browser.AccountCredit,
                ContactDetails = browserInfo.Browser.ToViewModel(),
                LastSearchedLocation = browserInfo.LastSearchLocation.ToViewModel()
            };

            ret.SetBrowserViewModel(browserInfo.Browser, blobStorage);
            return ret;
        }
    }
}