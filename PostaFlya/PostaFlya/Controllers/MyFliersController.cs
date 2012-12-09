﻿using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Models.Content;
using Website.Application.Binding;
using Website.Application.Content;
using PostaFlya.Areas.Default.Models.Bulletin;
using PostaFlya.Attributes;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Service;
using Website.Common.Extension;
using PostaFlya.Helpers;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Command;
using PostaFlya.Models;
using PostaFlya.Models.Factory;
using PostaFlya.Models.Flier;
using Website.Application.Domain.Content;
using Website.Domain.Tag;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.Controllers
{
    [BrowserAuthorize(Roles = "Participant")]
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
        public IQueryable<BulletinFlierModel> Get(string browserId)
        {
            var fliers = _queryService.GetByBrowserId<Flier>(browserId);
            return fliers.Select(_ => _viewModelFactory.GetBulletinViewModel(_, false)
                .GetImageUrl(_blobStorage))
                .AsQueryable();
        }

        // GET /api/Browser/browserId/myfliers/5
        public FlierCreateModel Get(string browserId, string id)
        {
            var flier = _queryService.FindById<Flier>(id);
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
                EffectiveDate = createModel.EffectiveDate,
                ImageList = createModel.ImageList.Select(_ => new FlierImage(_.ImageId)).ToList(),
                ExternalSource = createModel.ExternalSource,
                ExternalId = createModel.ExternalId,
                BoardSet = new HashSet<string>(createModel.BoardList),
                AllowUserContact = createModel.AllowUserContact,
                AttachTearOffs = createModel.AttachTearOffs,
                ExtendPostRadius = createModel.PostRadius
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
                EffectiveDate = editModel.EffectiveDate,
                ImageList = editModel.ImageList.Select(_ => new FlierImage(_.ImageId)).ToList(),
                BoardSet = new HashSet<string>(editModel.BoardList),
                AllowUserContact = editModel.AllowUserContact,
                AttachTearOffs = editModel.AttachTearOffs
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
