using System;
using System.Collections.Generic;
using System.Web.Http;
using WebSite.Application.Binding;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using PostaFlya.Helpers;
using PostaFlya.Models.Content;
using WebSite.Application.Content;
using Website.Domain.Content;
using Website.Domain.Content.Query;

namespace PostaFlya.Controllers
{
    public class MyImagesController : ApiController
    {
        private readonly ImageQueryServiceInterface _imageQueryService;
        private readonly BlobStorageInterface _blobStorage;

        public MyImagesController(ImageQueryServiceInterface imageQueryService,
            [ImageStorage]BlobStorageInterface blobStorage)
        {
            _imageQueryService = imageQueryService;
            _blobStorage = blobStorage;
        }

        public List<ImageViewModel> Get(string browserid)
        {
            return _imageQueryService.GetByBrowserId<Image>(browserid).Select(_ => _.ToViewModel().GetImageUrl(_blobStorage)).ToList();
        }
    }
}