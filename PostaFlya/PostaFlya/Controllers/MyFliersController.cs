using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Models.Content;
using Website.Application.Binding;
using Website.Application.Content;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using Website.Application.Domain.Browser;
using Website.Application.Domain.Browser.Query;
using Website.Application.Domain.Browser.Web;
using Website.Common.Controller;
using Website.Common.Extension;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Command;
using PostaFlya.Models.Factory;
using PostaFlya.Models.Flier;
using Website.Application.Domain.Content;
using Website.Domain.Tag;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.Controllers
{
    [BrowserAuthorizeHttp(Roles = "Participant")]
    public class MyFliersController : WebApiControllerBase
    {
        private readonly CommandBusInterface _commandBus;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierBehaviourViewModelFactoryInterface _viewModelFactory;
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;
        private readonly QueryChannelInterface _queryChannel;

        public MyFliersController(CommandBusInterface commandBus,
            GenericQueryServiceInterface queryService,
            [ImageStorage]BlobStorageInterface blobStorage
            , FlierBehaviourViewModelFactoryInterface viewModelFactory, PostaFlyaBrowserInformationInterface browserInformation, QueryChannelInterface queryChannel)
        {
            _commandBus = commandBus;
            _queryService = queryService;
            _blobStorage = blobStorage;
            _viewModelFactory = viewModelFactory;
            _browserInformation = browserInformation;
            _queryChannel = queryChannel;
        }

        // GET /api/myfliersapi
        public IList<BulletinFlierModel> Get(string browserId)
        {
            var fliers = _queryChannel.Query(new GetByBrowserIdQuery() {BrowserId = browserId}, new List<Flier>())
                .ToViewModel(_queryService, _blobStorage, _viewModelFactory);
            return fliers;
        }

        // GET /api/Browser/browserId/myfliers/5
        public FlierCreateModel Get(string browserId, string id)
        {
            var flier = _queryService.FindById<Flier>(id);
            if (flier != null && flier.BrowserId != browserId)
                return null;

            var flierModel = flier.ToCreateModel().GetImageUrl(_blobStorage);
            flierModel.ImageList.ForEach(_ => _.GetImageUrl(_blobStorage, ThumbOrientation.Vertical, ThumbSize.S114));
            return flierModel;
        }

        public HttpResponseMessage Post(string browserId, FlierCreateModel createModel)
        {
            var isAnon = createModel.Anonymous && _browserInformation.Browser.HasRole(Role.Admin);
            var createFlier = new CreateFlierCommand()
            {
                BrowserId = browserId,
                Tags = new Tags(createModel.TagsString),
                Title = createModel.Title.SafeText(),
                Description = createModel.Description.SafeText(),
                Image = new Guid(createModel.FlierImageId),
                FlierBehaviour = createModel.FlierBehaviour,
                EventDates = createModel.EventDates.Select(d => d.AsUnspecifiedDateTimeOffset()).ToList(),
                ImageList = createModel.ImageList.Select(_ => new FlierImage(_.ImageId)).ToList(),
                ExternalSource = createModel.ExternalSource,
                ExternalId = createModel.ExternalId,
                BoardSet = new HashSet<string>(createModel.BoardList),
                EnableAnalytics = createModel.EnableAnalytics,
                Venue = createModel.VenueInformation != null ? createModel.VenueInformation.ToDomainModel() : null,
                UserLinks = createModel.UserLinks == null? new List<UserLink>() : createModel.UserLinks.Select(_ => new UserLink(){Link = _.Link, Text = _.Text, Type = _.Type}).ToList(),
                Anonymous = isAnon
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
                Tags = new Tags(editModel.TagsString.EmptyIfNull()),
                Title = editModel.Title.SafeText(),
                Description = editModel.Description.SafeText(),
                Image = new Guid(editModel.FlierImageId),
                EventDates = editModel.EventDates.Select(d => d.AsUnspecifiedDateTimeOffset()).ToList(),
                ImageList = editModel.ImageList.Select(_ => new FlierImage(_.ImageId)).ToList(),
                BoardSet = new HashSet<string>(editModel.BoardList),
                EnableAnalytics = editModel.EnableAnalytics,
                Venue = editModel.VenueInformation != null ? editModel.VenueInformation.ToDomainModel() : null,
                UserLinks = editModel.UserLinks == null ? new List<UserLink>() : editModel.UserLinks.Select(_ => new UserLink() { Link = _.Link, Text = _.Text, Type = _.Type }).ToList()
            };

            var res = _commandBus.Send(editFlier);
            return this.GetResponseForRes(res);
        }

        public void Delete(string browserId, string id)
        {
            var flier = _queryService.FindById<Flier>(id);
            if (flier != null && flier.BrowserId != browserId)
                return;
            //TODO delete...
        }

    }
}
