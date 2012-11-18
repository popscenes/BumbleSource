using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Domain.Flier;
using Website.Application.Binding;
using PostaFlya.Areas.Default.Models.Bulletin;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Models.Browser;
using PostaFlya.Models.Factory;
using PostaFlya.Models.Flier;
using Website.Application.Content;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;
using Website.Domain.Claims;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class ProfileApiController : ApiController
    {
        private readonly QueryServiceForBrowserAggregateInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierBehaviourViewModelFactoryInterface _viewModelFactory;

        public ProfileApiController(QueryServiceForBrowserAggregateInterface queryService
            , [ImageStorage]BlobStorageInterface blobStorage
            , FlierBehaviourViewModelFactoryInterface viewModelFactory)
        {
            _queryService = queryService;
            _blobStorage = blobStorage;
            _viewModelFactory = viewModelFactory;
        }

        public ProfileViewModel Get(string handle)
        {
            return GetForHandle(handle, _queryService, _blobStorage, _viewModelFactory);
        }

        public static ProfileViewModel GetForHandle(string handle
            , QueryServiceForBrowserAggregateInterface queryService
            , BlobStorageInterface blobStorage
            , FlierBehaviourViewModelFactoryInterface viewModelFactory)
        {
            var browser = queryService.FindByFriendlyId<Browser>(handle);
            if (browser == null)
                return null;
            var ret = new ProfileViewModel
            {
                Browser = browser.ToViewModel(blobStorage),
                Fliers =
                    queryService.GetByBrowserId<Flier>(browser.Id)
                      .Select(f => viewModelFactory
                          .GetBulletinViewModel(f, false).GetImageUrl(blobStorage))
                      .ToList(),
                ClaimedFliers = queryService.GetByBrowserId<Claim>(browser.Id)
                      .Select(l => queryService.FindById<Flier>(l.AggregateId))
                      .Where(f => f != null)
                      .Select(f => viewModelFactory
                          .GetBulletinViewModel(f, false).GetImageUrl(blobStorage))
                      .ToList()
            };
            return ret;
        }
    }

    public class ProfileViewModel
    {
        public BrowserModel Browser { get; set; }
        public IList<BulletinFlierModel> Fliers { get; set; }
        public IList<BulletinFlierModel> ClaimedFliers { get; set; }
    }
}
