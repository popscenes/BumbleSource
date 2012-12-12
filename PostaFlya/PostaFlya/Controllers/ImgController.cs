using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Common.ActionResult;
using Website.Infrastructure.Command;
using PostaFlya.Models.Content;
using Website.Application.Domain.Browser;
using Website.Application.Domain.Content;
using Website.Domain.Content;
using Website.Domain.Content.Command;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class ImgController : Controller
    {
        private readonly BlobStorageInterface _blobStorage;
        private readonly RequestContentRetrieverFactoryInterface _contentRetrieverFactory;
        private readonly BrowserInformationInterface _browserInformation;
        private readonly CommandBusInterface _commandBus;
        private readonly GenericQueryServiceInterface _queryService;
        //private readonly HttpContextBase _httpContext;

        public ImgController([ImageStorage]BlobStorageInterface blobStorage
            , RequestContentRetrieverFactoryInterface contentRetrieverFactory
            , BrowserInformationInterface browserInformation, CommandBusInterface commandBus
            , GenericQueryServiceInterface queryService
            )
        {
            _blobStorage = blobStorage;
            _contentRetrieverFactory = contentRetrieverFactory;
            _browserInformation = browserInformation;
            _commandBus = commandBus;
            _queryService = queryService;
            //_httpContext = httpContext;
        }

        
        [HttpGet]
        public ActionResult GetError(string id)
        {
            var dotIndx = id.IndexOf('.');
            if (dotIndx >= 0)
                id = id.Remove(dotIndx);

            var idLength = Guid.NewGuid().ToString().Length;
            if (String.IsNullOrWhiteSpace(id) || id.Length < idLength)
                return File(GetNotFoundData(), "image/jpeg");

            id = id.Substring(0, idLength);
            var image = _queryService.FindById<Image>(id);
            if(image == null)
                return File(GetNotFoundData(), "image/jpeg");

            switch (image.Status)
            {   
                case ImageStatus.Failed:
                    return File(GetFailedProcessingData(), "image/jpeg");
                default:
                    return File(GetStillProcessingData(), "image/jpeg");
            }
        }

//        [HttpGet]
//        public ActionResult Get(string id, string view = "")
//        {
//            if (string.IsNullOrWhiteSpace(id))
//            {
//                Response.StatusCode = (int)HttpStatusCode.NotFound;
//                return new EmptyResult();
//            }
//
//            var image = _queryService.FindById(id);
//            if (image == null)
//            {
//                Response.StatusCode = (int)HttpStatusCode.NotFound;
//                return new EmptyResult();
//
//            }
//
//            if (image.Status == ImageStatus.Processing)
//            {
//                Response.StatusCode = (int)HttpStatusCode.NotFound;
//                return new EmptyResult();
//
//            }
//
//            if(image.Status == ImageStatus.Failed)
//            {
//                Response.StatusCode = (int)HttpStatusCode.NotFound;
//                return new EmptyResult();
//
//            }
//
//            if (_blobStorage.Exists(id + view))
//                SetClientCacheResponse();
//            else
//            {
//                Trace.TraceWarning("image {0} exists in table storage but not in blob storage for view {1}"
//                    ,id ,view);
//                //Response.StatusCode = (int)HttpStatusCode.NotFound;
//                return File(GetNotFoundData(), "image/jpeg");
//            }
//            
//            Action<Stream> write = stream => _blobStorage.GetToStream(id + view, stream);
//            return new WriteToStreamFileResult(write, "image/jpeg"); 
//        }

        // [HttpPost] //todo authorize browser.IsInRole(Role.Participant) [Authorize(Roles = "Participant")]
        //public HttpStatusCodeResult Post()
        //{
        //    return new HttpStatusCodeResult(HttpStatusCode.Created);
        //}

        [HttpPost] //todo authorize browser.IsInRole(Role.Participant) [Authorize(Roles = "Participant")]
        public HttpStatusCodeResult Post(ImageCreateModel createModel)
        {
            //plupload html4 runtime requires this
            Response.Write("{\"jsonrpc\" : \"2.0\", \"result\" : null, \"id\" : \"id\"}");
            
            var retriever = _contentRetrieverFactory.GetRetriever(Website.Domain.Content.Content.ContentType.Image);
            var content = retriever.GetContent();
            if(content == null || content.Data == null || content.Data.Length == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, retriever.GetLastError());

            var imgId = Guid.NewGuid();
            var res = _commandBus.Send(new CreateImageCommand()
                                 {
                                     CommandId = imgId.ToString(),
                                     Content = content,
                                     BrowserId = _browserInformation.Browser.Id,
                                     Title = createModel.Title,
                                     Location = createModel.Location != null ? createModel.Location.ToDomainModel() : null
                                 });

            Response.Headers["Location"] = Url != null ? Url.Action("Get", new {id = imgId}) : imgId.ToString();
            return new HttpStatusCodeResult(HttpStatusCode.Created);
        }


        [NonAction]
        public byte[] GetNotFoundData()
        {
            using (var ms = new MemoryStream())
            {
                PostaFlya.Properties.Resources.NotFoundImg.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        [NonAction]
        public byte[] GetStillProcessingData()
        {
            using (var ms = new MemoryStream())
            {
                PostaFlya.Properties.Resources.ImageProcessing.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        [NonAction]
        public byte[] GetFailedProcessingData()
        {
            using (var ms = new MemoryStream())
            {
                PostaFlya.Properties.Resources.ImageProcessingFailed.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        [NonAction]
        private bool Exists(string imageId)
        {
            var properties = _blobStorage.GetBlobProperties(imageId);
            return properties != null && !string.IsNullOrWhiteSpace(properties.ContentTyp);
        }

        [NonAction]
        private void SetClientCacheResponse()
        {
            var cachePolicy = ControllerContext.HttpContext.Response.Cache;
            cachePolicy.SetCacheability(HttpCacheability.Public);
            cachePolicy.VaryByParams["view"] = true;
            cachePolicy.VaryByParams["id"] = true;
            cachePolicy.SetOmitVaryStar(true);
            cachePolicy.SetExpires(DateTime.Now + TimeSpan.FromDays(365));
            cachePolicy.SetValidUntilExpires(true);
            var old = new DateTime(2001, 1, 1);
            cachePolicy.SetLastModified(old);
        }

        //todo local caching ...?
        //            var cachedir = _cacheDirService.Get(); // service could return HttpRuntime.CodegenDir/img/
        //            string cachePath = Path.Combine(cachedir, imageId);
        //            var fileInfo = new FileInfo(cachePath);
        //            if (fileInfo.Exists)
        //            {
        //                return System.IO.File.ReadAllBytes(fileInfo.Name);
        //            }
        //            else //get from blob and cache locally in _cacheDirService.Get()
        //            {
        //                
        //            }
    }
}