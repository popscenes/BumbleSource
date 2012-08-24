using System;
using System.Linq;
using System.Web.Mvc;
using PostaFlya.Application.Domain.ExternalSource;
using WebSite.Application.Content;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Flier.Query;
using WebSite.Infrastructure.Command;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using WebSite.Infrastructure.Authentication;
using WebSite.Application.Binding;
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

        protected string GetUrlCallBack(string providerIdentifier)
        {
            var callback = "http://localhost/";
            if (Url != null)
                callback = Url.Action("TokenResp", "FlierImport", new { providerIdentifier = providerIdentifier }, "http");

            //#if DEBUG
            callback = callback.Replace("82", "81");
            callback = callback.Replace("83", "81");
            //#endif

            return callback;
        }

        public ActionResult GetToken(string providerName)
        {
            var identityProvider = _identityProviderService.GetProviderByIdentifier(providerName);
            identityProvider.CallbackUrl = GetUrlCallBack(providerName);
            identityProvider.RequestAuthorisation();
            return new EmptyResult();

        }

        public ActionResult TokenResp(string providerName)
        {
            var identityProvider = _identityProviderService.GetProviderByIdentifier(providerName);
            var browserCreds = new BrowserIdentityProviderCredential()
                                   {
                                       BrowserId = _browserInfoService.Browser.Id
                                   };
            browserCreds.CopyFieldsFrom(identityProvider.GetCredentials());

            var command = new SetExternalCredentialCommand()
            {
                Credential = browserCreds
            };

            _commandBus.Send(command);

            return RedirectToAction("Import", "FlierImport");
        }

        public ActionResult Import(string providerName)
        {
            var flierImporter = _flierImportService.GetImporter(providerName);
            var browser = _browserInfoService.Browser;
            if(!flierImporter.CanImport(browser))
            {
                return View("GetToken");
            }

            var importedFliers = flierImporter.ImportFliers(browser);
            ViewBag.Fliers = importedFliers;

            var createFliers = importedFliers.Select(_ => _.ToCreateModel().GetDefaultImageUrl(_blobStorage, ThumbOrientation.Horizontal, ThumbSize.S250));
            ViewBag.Fliers = createFliers;
            return View(createFliers);
        }
    }
}