using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.WebsiteInformation;
using PostaFlya.Areas.Default.Models.Bulletin;
using PostaFlya.Attributes;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Service;
using Website.Common.Extension;
using PostaFlya.Helpers;
using Website.Infrastructure.Command;
using PostaFlya.Models;
using PostaFlya.Models.Factory;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Content;
using Website.Application.Domain.Content;
using Website.Domain.Tag;

namespace PostaFlya.Controllers
{
    [BrowserAuthorize(Roles = "Participant")]
    public class MyFliersController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly FlierQueryServiceInterface _flierQueryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly WebsiteInfoServiceInterface _websiteInfoService;
        private readonly FlierBehaviourViewModelFactoryInterface _viewModelFactory;

        public MyFliersController(CommandBusInterface commandBus,
            FlierQueryServiceInterface flierQueryService,
            [ImageStorage]BlobStorageInterface blobStorage
            , WebsiteInfoServiceInterface websiteInfoService
            , FlierBehaviourViewModelFactoryInterface viewModelFactory)
        {
            _commandBus = commandBus;
            _flierQueryService = flierQueryService;
            _blobStorage = blobStorage;
            _websiteInfoService = websiteInfoService;
            _viewModelFactory = viewModelFactory;
        }

        // GET /api/myfliersapi
        public IQueryable<BulletinFlierModel> Get(string browserId)
        {
            var fliers = _flierQueryService.GetByBrowserId<Flier>(browserId);
            return fliers.Select(_ => _viewModelFactory.GetBulletinViewModel(_, false)
                .GetImageUrl(_blobStorage))
                .AsQueryable();
        }

        // GET /api/Browser/browserId/myfliers/5
        public FlierCreateModel Get(string browserId, string id)
        {
            var flier = _flierQueryService.FindById<Flier>(id);
            if (flier != null && flier.BrowserId != browserId)
                return null;

            var flierModel = flier.ToCreateModel().GetDefaultImageUrl(_blobStorage, ThumbOrientation.Horizontal, ThumbSize.S250);
            flierModel.ImageList.ForEach(_ => _.GetDefaultImageUrl(_blobStorage, ThumbOrientation.Vertical, ThumbSize.S100));
            return flierModel;
        }

        public HttpResponseMessage Post(string browserId, FlierCreateModel createModel)
        {
            var createFlier = new CreateFlierCommand()
            {
                BrowserId = browserId,
                Tags = new Tags(createModel.TagsString),
                Title = createModel.Title.SafeText(),
                Description = createModel.Description.SafeText(),
                Location = createModel.Location.ToDomainModel(),
                Image = new Guid(createModel.FlierImageId),
                FlierBehaviour = createModel.FlierBehaviour,
                //WebSiteTags = _websiteInfoService.GetWebsiteTags(Request.RequestUri.Host),
                EffectiveDate = createModel.EffectiveDate,
                ImageList = createModel.ImageList.Select(_ => new FlierImage(_.ImageId)).ToList(),
                AttachContactDetails = createModel.AttachContactDetails,
                UseBrowserContactDetails = createModel.AttachContactDetails,//only supporting browser contact dets atm
                ExternalSource = createModel.ExternalSource,
                ExternalId = createModel.ExternalId
                //TODO add new details, and validate details both browser and new. Allow updating of browser details
            };

            var res = _commandBus.Send(createFlier);
            return this.GetResponseForRes(res);
        }

        public HttpResponseMessage Put(string browserId, FlierCreateModel editModel)
        {
            var editFlier = new EditFlierCommand()
            {
                Id = editModel.Id,
                BrowserId = browserId,
                Tags = new Tags(editModel.TagsString),
                Title = editModel.Title.SafeText(),
                Description = editModel.Description.SafeText(),
                Location = editModel.Location.ToDomainModel(),
                Image = new Guid(editModel.FlierImageId),
                EffectiveDate = editModel.EffectiveDate,
                ImageList = editModel.ImageList.Select(_ => new FlierImage(_.ImageId)).ToList()

            };

            var res = _commandBus.Send(editFlier);
            return this.GetResponseForRes(res);
        }

        public void Delete(string browserId, string id)
        {
            var flier = _flierQueryService.FindById<Flier>(id);
            if (flier != null && flier.BrowserId != browserId)
                return;
            //TODO delete...
        }

    }
}
