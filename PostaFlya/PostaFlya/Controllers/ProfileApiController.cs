using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Likes;
using WebSite.Application.Binding;
using PostaFlya.Application.Domain.Content;
using PostaFlya.Areas.Default.Models.Bulletin;
using PostaFlya.Domain.Browser.Query;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Models.Browser;
using PostaFlya.Models.Factory;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Likes;
using WebSite.Application.Content;

namespace PostaFlya.Controllers
{
    public class ProfileApiController : ApiController
    {
        private readonly BrowserQueryServiceInterface _browserQueryService;
        private readonly FlierQueryServiceInterface _flierQueryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierBehaviourViewModelFactoryInterface _viewModelFactory;

        public ProfileApiController(BrowserQueryServiceInterface browserQueryService
            , FlierQueryServiceInterface flierQueryService
            , [ImageStorage]BlobStorageInterface blobStorage
            , FlierBehaviourViewModelFactoryInterface viewModelFactory)
        {
            _browserQueryService = browserQueryService;
            _flierQueryService = flierQueryService;
            _blobStorage = blobStorage;
            _viewModelFactory = viewModelFactory;
        }

        public ProfileViewModel Get(string handle)
        {
            return GetForHandle(handle, _browserQueryService, _flierQueryService, _blobStorage, _viewModelFactory);
        }

        public static ProfileViewModel GetForHandle(string handle
            , BrowserQueryServiceInterface browserQueryService
            , FlierQueryServiceInterface flierQueryService
            , BlobStorageInterface blobStorage
            , FlierBehaviourViewModelFactoryInterface viewModelFactory)
        {
            var browser = browserQueryService.FindByHandle(handle);
            if (browser == null)
                return null;
            var ret = new ProfileViewModel
            {
                Browser = browser.ToViewModel(blobStorage),
                Fliers =
                    flierQueryService.GetByBrowserId<Flier>(browser.Id)
                      .Select(f => viewModelFactory
                          .GetBulletinViewModel(f, false).GetImageUrl(blobStorage))
                      .ToList(),
                LikedFliers = flierQueryService.GetByBrowserId<Like>(browser.Id)
                      .Select(l => flierQueryService.FindById<Flier>(l.AggregateId))
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
        public IList<BulletinFlierModel> LikedFliers { get; set; }
    }
}
