﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Models.Content;
using Website.Application.Binding;
using Website.Application.Content;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using Website.Application.Domain.Browser.Web;
using Website.Application.Domain.Obsolete;
using Website.Common.Extension;
using Website.Common.Model.Query;
using Website.Common.Obsolete;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Command;
using PostaFlya.Models.Flier;
using Website.Application.Domain.Content;
using Website.Domain.Tag;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.Controllers
{
    [Website.Application.Domain.Obsolete.BrowserAuthorizeHttp(Roles = "Participant")]
    public class MyFliersController : OldWebApiControllerBase
    {
        private readonly MessageBusInterface _messageBus;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;
        private readonly QueryChannelInterface _queryChannel;

        public MyFliersController(MessageBusInterface messageBus,
            GenericQueryServiceInterface queryService,
            [ImageStorage]BlobStorageInterface blobStorage
            , PostaFlyaBrowserInformationInterface browserInformation, QueryChannelInterface queryChannel)
        {
            _messageBus = messageBus;
            _queryService = queryService;
            _blobStorage = blobStorage;
            _browserInformation = browserInformation;
            _queryChannel = queryChannel;
        }

        // GET /api/myfliersapi
        public IList<BulletinFlierSummaryModel> Get(string browserId)
        {
            return _queryChannel.Query(new GetByBrowserIdQuery<Flier>() { BrowserId = browserId }, new List<BulletinFlierSummaryModel>());
        }

        // GET /api/Browser/browserId/myfliers/5
        public FlierCreateModel Get(string browserId, string id)
        {
            var flier = _queryService.FindById<Flier>(id);
            if (flier != null && flier.BrowserId != browserId)
                return null;
            return _queryChannel.Query(new FindByIdQuery<Flier>{Id = id}, (FlierCreateModel)null);
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
                //Venue = createModel.VenueInformation != null ? createModel.VenueInformation.ToDomainModel() : null,
                UserLinks = createModel.UserLinks == null? new List<UserLink>() : createModel.UserLinks.Select(_ => new UserLink(){Link = _.Link, Text = _.Text, Type = _.Type}).ToList(),
                Anonymous = isAnon
            };

            var res = _messageBus.Send(createFlier);
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
                //Venue = editModel.VenueInformation != null ? editModel.VenueInformation.ToDomainModel() : null,
                UserLinks = editModel.UserLinks == null ? new List<UserLink>() : editModel.UserLinks.Select(_ => new UserLink() { Link = _.Link, Text = _.Text, Type = _.Type }).ToList()
            };

            var res = _messageBus.Send(editFlier);
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
