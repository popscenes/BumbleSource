using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PostaFlya.Application.Domain.SiteMap;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Common.ActionResult;

namespace PostaFlya.Controllers
{
    public class SiteMapController : Controller
    {
        private readonly BlobStorageInterface _blobStorage;

        public SiteMapController([ApplicationStorage]BlobStorageInterface blobStorage)
        {
            _blobStorage = blobStorage;
        }

        //
        // GET: /sitemap.xml

        public ActionResult Index(string sitemap)
        {
            if (!sitemap.Contains("sitemap") || !_blobStorage.Exists(sitemap))
                return HttpNotFound("File Not Found");

            Action<Stream> write = stream => _blobStorage.GetToStream(sitemap, stream);
            return new WriteToStreamFileResult(write, "application/xml"); 
        }

    }
}
