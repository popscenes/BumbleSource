using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Website.Application.Binding;
using PostaFlya.Models.Content;
using Website.Application.Content;
using Website.Application.Domain.Browser.Web;
using Website.Application.Domain.Obsolete;
using Website.Common.Obsolete;
using Website.Domain.Browser.Query;
using Website.Domain.Content;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    [Website.Application.Domain.Obsolete.BrowserAuthorizeHttp]
    public class MyImagesController : OldWebApiControllerBase
    {
        private readonly BlobStorageInterface _blobStorage;
        private readonly QueryChannelInterface _queryChannel;

        public MyImagesController(
            [ImageStorage]BlobStorageInterface blobStorage, QueryChannelInterface queryChannel)
        {
            _blobStorage = blobStorage;
            _queryChannel = queryChannel;
        }

        public List<ImageViewModel> Get(string browserid)
        {
            return _queryChannel.Query(new GetByBrowserIdQuery<Image>() {BrowserId = browserid}, new List<Image>())
                                .Select(_ => _.ToViewModel().GetImageUrl(_blobStorage, false)).ToList();
        }
    }
}