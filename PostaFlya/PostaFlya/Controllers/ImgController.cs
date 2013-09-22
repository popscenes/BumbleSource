using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Application.Domain.Flier;
using PostaFlya.Domain.Flier;
using Website.Application.Binding;
using Website.Application.Content;
using Website.Application.Extension.Content;
using Website.Common.ActionResult;
using Website.Domain.Browser;
using Website.Infrastructure.Command;
using PostaFlya.Models.Content;
using Website.Application.Domain.Browser;
using Website.Application.Domain.Content;
using Website.Domain.Content;
using Website.Domain.Content.Command;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    public class ImgController : Controller
    {
        private readonly BlobStorageInterface _blobStorage;
        private readonly RequestContentRetrieverFactoryInterface _contentRetrieverFactory;
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;
        private readonly MessageBusInterface _messageBus;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly FlierPrintImageServiceInterface _flierPrintImageService;
        //private readonly HttpContextBase _httpContext;

        public ImgController([ImageStorage]BlobStorageInterface blobStorage
            , RequestContentRetrieverFactoryInterface contentRetrieverFactory
            , PostaFlyaBrowserInformationInterface browserInformation, MessageBusInterface messageBus
            , GenericQueryServiceInterface queryService, 
            FlierPrintImageServiceInterface flierPrintImageService)
        {
            _blobStorage = blobStorage;
            _contentRetrieverFactory = contentRetrieverFactory;
            _browserInformation = browserInformation;
            _messageBus = messageBus;
            _queryService = queryService;
            _flierPrintImageService = flierPrintImageService;
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

        [HttpPost] //todo authorize browser.IsInRole(Role.Participant) [Authorize(Roles = "Participant")]
        public HttpStatusCodeResult Post(ImageCreateModel createModel)
        {
            //plupload html4 runtime requires this
            
            
            var retriever = _contentRetrieverFactory.GetRetriever(Website.Domain.Content.Content.ContentType.Image);
            var content = retriever.GetContent();
            if(content == null || content.Data == null || content.Data.Length == 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, retriever.GetLastError());

            var imgId = Guid.NewGuid();
            _messageBus.Send(new CreateImageCommand()
                                 {
                                     MessageId = imgId.ToString(),
                                     Content = content,
                                     BrowserId = _browserInformation.Browser.Id,
                                     Title = createModel.Title,
                                     Location = createModel.Location != null ? createModel.Location.ToDomainModel() : null,
                                     Anonymous = createModel.Anonymous && _browserInformation.Browser.HasRole(Role.Admin),
                                     KeepFileImapeType = createModel.KeepFileImapeType
                                 });

            var uri = _blobStorage.GetBlobUri(imgId + ImageUtil.GetIdFileExtension(createModel.KeepFileImapeType, content.Extension), false);
            Response.Headers["Location"] = uri != null ? uri.ToString() : imgId.ToString();
            Response.Write("{\"jsonrpc\" : \"2.0\", \"result\" : null, \"id\" : \"" + imgId + "\", \"url\": \"" + uri + "\"}");
            return new HttpStatusCodeResult(HttpStatusCode.Created);
        }


        [NonAction]
        public byte[] GetNotFoundData()
        {
            return Properties.Resources.NotFoundImg.GetBytes(ImageFormat.Jpeg);
        }

        [NonAction]
        public byte[] GetStillProcessingData()
        {
            return Properties.Resources.ImageProcessing.GetBytes(ImageFormat.Jpeg);
        }

        [NonAction]
        public byte[] GetFailedProcessingData()
        {
            return Properties.Resources.ImageProcessingFailed.GetBytes(ImageFormat.Jpeg);
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

        public ActionResult ImgRet(string imageUrl)
        {
            var aRequest = (HttpWebRequest)WebRequest.Create(imageUrl);
            var aResponse = (HttpWebResponse)aRequest.GetResponse();


            Response.Headers.Add("access-control-allow-origin", "*");
            Response.Headers.Add("access-control-allow-credentials", "true");
            return new FileStreamResult(aResponse.GetResponseStream(), "image/jpeg");
        }

        public const string TearOffPrintStyle = "tearoff";
        public const string CodeOnlyPrintStyle = "codeonly";
        public const string TopLeftPrintStyle = "topleft";
        public const string TopRightPrintStyle = "topright";
        public const string BottomLeftPrintStyle = "bottomleft";
        public const string BottomRightPrintStyle = "bottomright";

        public ActionResult GetPrintFlier(string id, string printStyle = TearOffPrintStyle)
        {
            var flier = _queryService.FindById<Flier>(id);
            if(flier == null)
                return new HttpNotFoundResult();


            System.Drawing.Image img = null;

            if (printStyle.Equals(TearOffPrintStyle, StringComparison.CurrentCultureIgnoreCase))
                img = _flierPrintImageService.GetPrintImageForFlierWithTearOffs(id);

            if (printStyle.Equals(CodeOnlyPrintStyle, StringComparison.CurrentCultureIgnoreCase))
                img = _flierPrintImageService.GetQrCodeImageForFlier(id);

            if (printStyle.Equals(TopLeftPrintStyle, StringComparison.CurrentCultureIgnoreCase))
                img = _flierPrintImageService.GetPrintImageForFlier(id, FlierPrintImageServiceQrLocation.TopLeft);
            if (printStyle.Equals(TopRightPrintStyle, StringComparison.CurrentCultureIgnoreCase))
                img = _flierPrintImageService.GetPrintImageForFlier(id, FlierPrintImageServiceQrLocation.TopRight);
            if (printStyle.Equals(BottomLeftPrintStyle, StringComparison.CurrentCultureIgnoreCase))
                img = _flierPrintImageService.GetPrintImageForFlier(id, FlierPrintImageServiceQrLocation.BottomLeft);
            if (printStyle.Equals(BottomRightPrintStyle, StringComparison.CurrentCultureIgnoreCase))
                img = _flierPrintImageService.GetPrintImageForFlier(id, FlierPrintImageServiceQrLocation.BottomRight);

            if(img == null)
                return new HttpNotFoundResult();

            var ret =  File(img.GetBytes(ImageFormat.Jpeg), "image/jpeg", flier.FriendlyId + ".jpg");
            
            img.Dispose();
            
            return ret;
        }
    }
}