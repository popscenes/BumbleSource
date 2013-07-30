using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Models.Flier;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Domain.Browser.Web;
using Website.Common.Extension;
using Website.Common.Model.Query;
using Website.Common.Obsolete;
using Website.Domain.Browser.Query;
using Website.Domain.Tag;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
     [BrowserAuthorizeHttp(Roles = "Participant")]
    public class PendingFliersApiController : OldWebApiControllerBase
    {
         private readonly PostaFlyaBrowserInformationInterface _browserInformation;
        private readonly GenericQueryServiceInterface _browserQueryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly CommandBusInterface _commandBus;
         private readonly QueryChannelInterface _queryChannel;

        public PendingFliersApiController(PostaFlyaBrowserInformationInterface browserInformation
            , GenericQueryServiceInterface browserQueryService, 
            [ImageStorage]BlobStorageInterface blobStorage
            , GenericQueryServiceInterface queryService
            , CommandBusInterface commandBus, QueryChannelInterface queryChannel)
        {
            _browserInformation = browserInformation;
            _browserQueryService = browserQueryService;
            _blobStorage = blobStorage;
            _queryService = queryService;
            _commandBus = commandBus;
            _queryChannel = queryChannel;
        }

        // GET api/pendingfliersapi
        public IEnumerable<BulletinFlierSummaryModel> Get()
        {
            var fliers = _queryChannel.Query(new GetByBrowserIdQuery<Flier>() {BrowserId = _browserInformation.Browser.Id}, new List<Flier>());
            var pendingFliers = fliers.Where(_ => _.Status == FlierStatus.PaymentPending);
            var model = _queryChannel.ToViewModel<BulletinFlierSummaryModel, Flier>(pendingFliers);
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
                Image = flier.Image,
                EventDates = flier.EventDates,
                ImageList = flier.ImageList,
                //BoardSet = flier.Boards,
                AllowUserContact = flier.HasLeadGeneration,
                EnableAnalytics = flier.EnableAnalytics,
                ExtendPostRadius = flier.LocationRadius
            };

            var res = _commandBus.Send(editFlier);
            return this.GetResponseForRes(res);
        }
    }
}
