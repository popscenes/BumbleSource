using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Website.Application.Binding;
using System.Linq.Expressions;
using System.Collections;
using PostaFlya.Helpers;
using PostaFlya.Models.Content;
using Website.Application.Content;
using Website.Domain.Browser.Query;
using Website.Domain.Content;

namespace PostaFlya.Controllers
{
    public class MyImagesController : ApiController
    {
        private readonly QueryServiceForBrowserAggregateInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;

        public MyImagesController(QueryServiceForBrowserAggregateInterface queryService,
            [ImageStorage]BlobStorageInterface blobStorage)
        {
            _queryService = queryService;
            _blobStorage = blobStorage;
        }

        public List<ImageViewModel> Get(string browserid)
        {
            return _queryService.GetByBrowserId<Image>(browserid).Select(_ => _.ToViewModel().GetImageUrl(_blobStorage)).ToList();
        }
    }
}