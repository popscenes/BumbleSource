using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Models.Factory;
using PostaFlya.Models.Flier;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Domain.Browser;
using Website.Application.Domain.Browser.Web;
using Website.Common.Extension;
using Website.Domain.Browser.Query;
using Website.Domain.Tag;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
     [BrowserAuthorize(Roles = "Participant")]
    public class PendingFliersApiController : ApiController
    {
        private readonly BrowserInformationInterface _browserInformation;
        private readonly QueryServiceForBrowserAggregateInterface _browserQueryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierBehaviourViewModelFactoryInterface _viewModelFactory;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly CommandBusInterface _commandBus;

        public PendingFliersApiController(BrowserInformationInterface browserInformation
            , QueryServiceForBrowserAggregateInterface browserQueryService, 
            [ImageStorage]BlobStorageInterface blobStorage
            , FlierBehaviourViewModelFactoryInterface viewModelFactory
            , GenericQueryServiceInterface queryService
            , CommandBusInterface commandBus)
        {
            _browserInformation = browserInformation;
            _browserQueryService = browserQueryService;
            _blobStorage = blobStorage;
            _viewModelFactory = viewModelFactory;
            _queryService = queryService;
            _commandBus = commandBus;
        }

        // GET api/pendingfliersapi
        public IEnumerable<BulletinFlierModel> Get()
        {
            var fliers = _browserQueryService.GetByBrowserId<Flier>(_browserInformation.Browser.Id);
            var pendingFliers = fliers.Where(_ => _.Status == FlierStatus.PaymentPending);
            var model = pendingFliers.Select(_ => _viewModelFactory.GetBulletinViewModel(_, false).GetImageUrl(_blobStorage)).ToList();
            return model;
        }
        // PUT api/pendingfliersapi/5
        public HttpResponseMessage Put(string browserid, String id)
        {
            var flier = _queryService.FindById<Flier>(id);

            var editFlier = new EditFlierCommand()
            {
                Id = flier.Id,
                BrowserId = flier.BrowserId,
                Tags = new Tags(flier.Tags),
                Title = flier.Title,
                Description = flier.Description,
                Location = flier.Location,
                Image = flier.Image,
                EffectiveDate = flier.EffectiveDate,
                ImageList = flier.ImageList,
                BoardSet = flier.Boards,
                AllowUserContact = flier.HasLeadGeneration
            };

            var res = _commandBus.Send(editFlier);
            return this.GetResponseForRes(res);
        }
    }
}
