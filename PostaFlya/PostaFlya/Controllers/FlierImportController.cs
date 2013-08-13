using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Application.Domain.ExternalSource;
using PostaFlya.Domain.Flier;
using PostaFlya.Models;
using Website.Application.Content;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Flier.Query;
using Website.Common.Model.Query;
using Website.Domain.Content;
using Website.Infrastructure.Command;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using Website.Infrastructure.Authentication;
using Website.Application.Binding;
using Website.Application.Domain.Browser;
using Website.Application.Domain.Content;
using Website.Domain.Browser;
using Website.Domain.Browser.Command;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;

namespace PostaFlya.Controllers
{
    [Authorize(Roles = "Participant")]
    public class FlierImportController : Controller
    {
        private readonly PostaFlyaBrowserInformationInterface _browserInfoService;
        private readonly FlierImportServiceInterface _flierImportService;
        private IdentityProviderServiceInterface _identityProviderService;
        private MessageBusInterface _messageBus;
        private readonly BlobStorageInterface _blobStorage;
        private readonly QueryChannelInterface _queryChannel;

        public FlierImportController(PostaFlyaBrowserInformationInterface browserInfoService, FlierImportServiceInterface flierImportService,
            IdentityProviderServiceInterface identityProviderService, MessageBusInterface messageBus, [ImageStorage]BlobStorageInterface blobStorage, QueryChannelInterface queryChannel)
        {
            _browserInfoService = browserInfoService;
            _flierImportService = flierImportService;
            _identityProviderService = identityProviderService;
            _messageBus = messageBus;
            _blobStorage = blobStorage;
            _queryChannel = queryChannel;
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
                                        new { providerIdentifier = providerName, callbackAction = "Import", callbackController = "FlierImport" });
            }

            var importedFliers = flierImporter.ImportFliers(browser);
            var createFliers = _queryChannel.ToViewModel<FlierCreateModel, FlierInterface>(importedFliers);
            ViewBag.Fliers = createFliers;
            var model = new flierImportModel() { CreatedFliers = createFliers.AsQueryable(), PageId = WebConstants.FlierImportPage };

            return View(model);
        }
    }

    public class flierImportModel : PageModelInterface
    {
        public string PageId { get; set; }
        public string ActiveNav { get; set; }

        public IQueryable<FlierCreateModel> CreatedFliers { get; set; }
    }
}