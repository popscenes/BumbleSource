using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Models.Content;
using Website.Application.Binding;
using Website.Application.Content;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using Website.Application.Domain.Browser.Web;
using Website.Common.Extension;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Command;
using PostaFlya.Models.Factory;
using PostaFlya.Models.Flier;
using Website.Application.Domain.Content;
using Website.Domain.Tag;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.Controllers
{
    [BrowserAuthorizeHttp(Roles = "Participant")]
    public class MyFliersController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly QueryServiceForBrowserAggregateInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly FlierBehaviourViewModelFactoryInterface _viewModelFactory;

        public MyFliersController(CommandBusInterface commandBus,
            QueryServiceForBrowserAggregateInterface queryService,
            [ImageStorage]BlobStorageInterface blobStorage
            , FlierBehaviourViewModelFactoryInterface viewModelFactory)
        {
            _commandBus = commandBus;
            _queryService = queryService;
            _blobStorage = blobStorage;
            _viewModelFactory = viewModelFactory;
        }

        // GET /api/myfliersapi
        public IList<BulletinFlierModel> Get(string browserId)
        {
            var fliers = _queryService.GetByBrowserId<Flier>(browserId).ToViewModel(_queryService, _blobStorage, _viewModelFactory);
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
            var createFlier = new CreateFlierCommand()
            {
                BrowserId = browserId,
                Tags = new Tags(createModel.TagsString),
                Title = createModel.Title.SafeText(),
                Description = createModel.Description.SafeText(),
                Location = createModel.Location.ToDomainModel(),
                Image = new Guid(createModel.FlierImageId),
                FlierBehaviour = createModel.FlierBehaviour,
                EventDates = createModel.EventDates.Select(d => d.ToUniversalTime()).ToList(),
                ImageList = createModel.ImageList.Select(_ => new FlierImage(_.ImageId)).ToList(),
                ExternalSource = createModel.ExternalSource,
                ExternalId = createModel.ExternalId,
                BoardSet = new HashSet<string>(createModel.BoardList),
                //AllowUserContact = createModel.AllowUserContact,
                ExtendPostRadius = Math.Max(0, createModel.PostRadius - 5),
                EnableAnalytics = createModel.EnableAnalytics,
                ContactDetails = createModel.VenueInformation != null ? createModel.VenueInformation.ToDomainModel() : null,
                UserLinks = createModel.UserLinks == null? new List<UserLink>() : createModel.UserLinks.Select(_ => new UserLink(){Link = _.Link, Text = _.Text, Type = _.Type}).ToList()
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
                Location = editModel.Location.ToDomainModel(),
                Image = new Guid(editModel.FlierImageId),
                EventDates = editModel.EventDates.Select(d => d.ToUniversalTime()).ToList(),
                ImageList = editModel.ImageList.Select(_ => new FlierImage(_.ImageId)).ToList(),
                BoardSet = new HashSet<string>(editModel.BoardList),
                //AllowUserContact = editModel.AllowUserContact,
                ExtendPostRadius = Math.Max(0, editModel.PostRadius - 5),
                EnableAnalytics = editModel.EnableAnalytics,
                ContactDetails = editModel.VenueInformation != null ? editModel.VenueInformation.ToDomainModel() : null,
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
