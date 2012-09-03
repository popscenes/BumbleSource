using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using PostaFlya.Application.Domain.ExternalSource;
using Website.Application.Content;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Flier.Query;
using Website.Infrastructure.Command;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using Website.Infrastructure.Authentication;
using Website.Application.Binding;
using Website.Application.Domain.Browser;
using Website.Application.Domain.Content;
using Website.Domain.Browser;
using Website.Domain.Browser.Command;

namespace PostaFlya.Controllers
{
    [Authorize(Roles = "Participant")]
    public class FlierImportController : Controller
    {
        private BrowserInformationInterface _browserInfoService;
        private FlierImportServiceInterface _flierImportService;
        private IdentityProviderServiceInterface _identityProviderService;
        private CommandBusInterface _commandBus;
        private readonly BlobStorageInterface _blobStorage;

        public FlierImportController(BrowserInformationInterface browserInfoService, FlierImportServiceInterface flierImportService,
            IdentityProviderServiceInterface identityProviderService, CommandBusInterface commandBus, [ImageStorage]BlobStorageInterface blobStorage)
        {
            _browserInfoService = browserInfoService;
            _flierImportService = flierImportService;
            _identityProviderService = identityProviderService;
            _commandBus = commandBus;
            _blobStorage = blobStorage;
        }

        //protected string GetUrlCallBack(string providerIdentifier)
        //{
        //    var callback = "http://localhost/";
        //    if (Url != null)
        //        callback = Url.Action("TokenResp", "FlierImport", new { providerIdentifier = providerIdentifier }, "http");

        //    //#if DEBUG
        //    callback = callback.Replace("82", "81");
        //    callback = callback.Replace("83", "81");
        //    //#endif

        //    return callback;
        //}

        //public ActionResult GetToken(string providerName)
        //{
        //    var identityProvider = _identityProviderService.GetProviderByIdentifier(providerName);
        //    identityProvider.CallbackUrl = GetUrlCallBack(providerName);
        //    identityProvider.RequestAuthorisation();
        //    return new EmptyResult();

        //}


        public ActionResult Import(string providerName)
        {
            var flierImporter = _flierImportService.GetImporter(providerName);
            var browser = _browserInfoService.Browser;
            if(!flierImporter.CanImport(browser))
            {
                return RedirectToAction("RequestToken", "Account",
                                        new { providerIdentifier = providerName, callbackAction = "Import", callbackController = "FlierImportController" });
            }

            var importedFliers = flierImporter.ImportFliers(browser);
            ViewBag.Fliers = importedFliers;

            var createFliers = importedFliers.Select(_ => _.ToCreateModel().GetDefaultImageUrl(_blobStorage, ThumbOrientation.Horizontal, ThumbSize.S250));
            ViewBag.Fliers = createFliers;
            return View(createFliers);
        }
    }
}