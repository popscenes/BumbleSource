using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Areas.WebApi.Location.Model;
using PostaFlya.Domain.Boards.Query;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Board;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using Website.Application.Content;
using Website.Application.Domain.Browser;
using Website.Common.Model;
using Website.Common.Model.Query;
using Website.Domain.Contact;
using Website.Domain.Location;
using Website.Infrastructure.Query;

namespace PostaFlya.Models.Browser
{
    public class ToCurrentBrowserModel : ViewModelMapperInterface<CurrentBrowserModel, PostaFlyaBrowserInformationInterface>
    {
        private readonly QueryChannelInterface _queryChannel;

        public ToCurrentBrowserModel(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public CurrentBrowserModel ToViewModel(CurrentBrowserModel target, PostaFlyaBrowserInformationInterface source)
        {
            if (target == null) 
                target = new CurrentBrowserModel();
    
            target.Handle = source.Browser.FriendlyId;
            target.BrowserId = source.Browser.Id;
            target.Roles = source.Browser.Roles.Select(r => r).ToList();
            target.LastSearchedLocation = source.LastSearchLocation== null ? null : _queryChannel.ToViewModel<SuburbModel, Suburb>(source.LastSearchLocation);
            target.AdminBoards = new List<BoardSummaryModel>();
            /*if (source.PostaBrowser.AdminBoards != null && source.PostaBrowser.AdminBoards.Count > 0)
            target.AdminBoards =
                _queryChannel.Query(new FindBoardByAdminEmailQuery() { AdminEmail  = source.Browser.EmailAddress},
                                      (List<BoardModel>) null);*/


            _queryChannel.ToViewModel<BrowserModel, Domain.Browser.Browser>(source.PostaBrowser, target);


            return target;
        }
    }
    [Serializable]
    [DataContract]
    public class CurrentBrowserModel : BrowserModel
    {
        [DataMember]
        public String BrowserId { get; set; }
        [DataMember]
        public List<LocationModel> SavedLocations { get; set; }
        [DataMember]
        public List<BoardSummaryModel> AdminBoards { get; set; }
        [DataMember]
        public List<String> SavedTags { get; set; }
        [DataMember]
        public List<string> Roles { get; set; }
        [DataMember]
        public double Credits { get; set; }
        [DataMember]
        public ContactDetailsModel ContactDetails { get; set; }
        [DataMember]
        public SuburbModel LastSearchedLocation { get; set; }
    }

//    public static class BulletinFlierModelFlierInterfaceExtension
//    {
//        public static CurrentBrowserModel ToCurrentBrowserModel(this PostaFlyaBrowserInformationInterface browserInfo, BlobStorageInterface blobStorage)
//        {
//            var ret =  new CurrentBrowserModel()
//            {
//                Handle = browserInfo.Browser.FriendlyId,
//                BrowserId = browserInfo.Browser.Id,
//                SavedLocations = browserInfo.Browser.SavedLocations.Select(_ => _.ToViewModel()).ToList(),
//                SavedTags = browserInfo.Browser.SavedTags.Select(_ => _.ToString()).ToList(),
//                Roles = browserInfo.Browser.Roles.Select(r => r).ToList(),
//                Credits = browserInfo.Browser.AccountCredit,
//                ContactDetails = browserInfo.Browser.ToViewModel(),
//                LastSearchedLocation = browserInfo.LastSearchLocation.ToViewModel()
//            };
//
//            
//            return ret;
//        }
//    }
}