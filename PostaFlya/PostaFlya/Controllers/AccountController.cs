using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Domain.Browser.Command;
using PostaFlya.Models.Account;
using Website.Application.Authentication;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Command;
using PostaFlya.Models;
using Website.Application.Domain.Browser;
using Website.Domain.Browser.Command;
using Website.Domain.Location;
using Website.Infrastructure.Configuration;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;
using Browser = PostaFlya.Domain.Browser.Browser;
using Roles = Website.Domain.Browser.Roles;

namespace PostaFlya.Controllers
{

    
    public class AccountController : Controller
    {
        private readonly IdentityProviderServiceInterface _identityProviderService;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly MessageBusInterface _messageBus;
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;
        private readonly ConfigurationServiceInterface _configurationService;
        private readonly QueryChannelInterface _queryChannel;

        public AccountController(IdentityProviderServiceInterface identityProviderService, GenericQueryServiceInterface queryService,
            MessageBusInterface messageBus, PostaFlyaBrowserInformationInterface browserInformation, ConfigurationServiceInterface configurationService, QueryChannelInterface queryChannel)
        {
            _identityProviderService = identityProviderService;
            _queryService = queryService;
            _messageBus = messageBus;
            _browserInformation = browserInformation;
            _configurationService = configurationService;
            _queryChannel = queryChannel;
        }

        public ActionResult LoginPage(string ReturnUrl = null)
        {
            ViewBag.ReturnUrl = ReturnUrl;
            ViewBag.FreeCredits = _configurationService.GetSetting("NewAccountCredit");
            return View("Login", new LoginPageModel(){PageId = WebConstants.AccountLoginPage});
        }

        [Authorize]
        public ActionResult AddBrowser(Browser browser)
        {
            var command = new AddBrowserCommand()
            {
               Browser = browser
            };

            _messageBus.Send(command);
            return View("Home");
        }


        protected string GetUrlCallBack(string providerIdentifier, string controller, string action, string ReturnUrl)
        {
            var callback = "http://localhost/";
            if (Url != null)
                callback = _configurationService.GetSetting("SiteUrl") + Url.Action(action, controller, new {providerIdentifier, ReturnUrl });


            return callback;
        }

        public ActionResult AuthRequest(string providerIdentifier, string ReturnUrl = null)
        {
            var identityProvider = SetUpIdentityProvider(providerIdentifier, "Account", "AuthResponse", ReturnUrl);
            identityProvider.RequestAuthorisation();
            return new EmptyResult();

        }

        [ValidateInput(false)]
        public ActionResult AuthResponse(string providerIdentifier, string ReturnUrl = null)
        {
            var identityProvider = _identityProviderService.GetProviderByIdentifier(providerIdentifier);
            var callback = GetUrlCallBack(providerIdentifier, "Account", "AuthResponse", ReturnUrl);
            identityProvider.CallbackUrl = callback;
            var identityProviderCredentials = identityProvider.GetCredentials();
            if (identityProviderCredentials == null)
            {
                return RedirectToAction("LoginPage", "Account", new {ReturnUrl});
            }

            SetBrowserFromIdentityProviderCredentials(identityProviderCredentials);
            CreateFormsCookieFromIdenityProviderCredentials(identityProviderCredentials);

            if (!string.IsNullOrWhiteSpace(ReturnUrl) && ReturnUrl.StartsWith("/"))
                return Redirect(ReturnUrl);

            return RedirectToAction("GigGuide", "Bulletin");
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("GigGuide", "Bulletin");
        }

        [NonAction]
        protected void CreateFormsCookieFromIdenityProviderCredentials(IdentityProviderCredential identityProviderCredentials)
        {
            var authTicket = new
                            FormsAuthenticationTicket(1, //version
                            identityProviderCredentials.Name, // user name
                            DateTime.Now,             //creation
                            DateTime.Now.AddMinutes(30), //Expiration
                            false, //Persistent
                            identityProviderCredentials.ToString());

            string encTicket = FormsAuthentication.Encrypt(authTicket);

            this.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encTicket));
        }

        [NonAction]
        [Authorize]
        public void SetBrowserFromIdentityProviderCredentials(IdentityProviderCredential identityProviderCredentials)
        {

            var browserAsParticipant =
                _queryChannel.Query(
                    new FindBrowserByIdentityProviderQuery() {Credential = identityProviderCredentials}, (Browser) null);

            if (browserAsParticipant == null)
            {
                CreateBrowserFromIdentityProviderCredentials(identityProviderCredentials);
                browserAsParticipant = _queryChannel.Query(
                    new FindBrowserByIdentityProviderQuery() { Credential = identityProviderCredentials }, (Browser)null);
            }

            //just set the current browser as the browser
//            if (!browserAsParticipant.Equals(_browserInformation.Browser))
//                _browserInformation.Browser = browserAsParticipant;


        }

        [NonAction]
        public void CreateBrowserFromIdentityProviderCredentials(IdentityProviderCredential identityProviderCredentials)
        {
            var roles = new Website.Domain.Browser.Roles {Role.Participant.ToString()};
#if DEBUG
            roles.Add(Role.Admin.ToString());
#endif
            var command = new AddBrowserCommand()
            {
                Browser = new PostaFlya.Domain.Browser.Browser()
                {
                    Id = Guid.NewGuid().ToString(),
                    FriendlyId = identityProviderCredentials.Name,
                    EmailAddress = identityProviderCredentials.Email,
                    Roles = roles,
                    AccountCredit = _configurationService.GetSetting<double>("NewAccountCredit")
                }
            };

            var creds = new BrowserIdentityProviderCredential(){BrowserId = command.Browser.Id};
            creds.CopyFieldsFrom(identityProviderCredentials);
            command.Browser.ExternalCredentials = new HashSet<BrowserIdentityProviderCredential>(){creds};

            _messageBus.Send(command);
        }

        [Authorize]
        [ValidateInput(false)]
        public ActionResult Authenticate(FormCollection formCollection)
        {
            var principal = ((AzureWebPrincipal)User);
            var browserAsParticipant = _queryChannel.Query(
                    new FindBrowserByIdentityProviderQuery() { Credential = principal.ToCredential() }, (Browser)null);
                
            if (browserAsParticipant == null)
            {
                CreateBrowser(principal);
                browserAsParticipant = _queryChannel.Query(
                    new FindBrowserByIdentityProviderQuery() { Credential = principal.ToCredential() }, (Browser)null);
            }

            //just set the current browser as the browser
//            if (!browserAsParticipant.Equals(_browserInformation.Browser))
//                _browserInformation.Browser = browserAsParticipant;

            var target = formCollection.Get("ReturnUrl");
            if (!string.IsNullOrWhiteSpace(target) && target.StartsWith("/"))
                return Redirect(target);

            return RedirectToAction("GigGuide", "Bulletin");
        }

        [NonAction]
        protected void CreateBrowser(AzureWebPrincipal principal)
        {
            var command = new AddBrowserCommand()
                              {
                                  Browser = new Browser()
                                                {
                                                    Id = Guid.NewGuid().ToString(),
                                                    FriendlyId = principal.Name,
                                                    EmailAddress = principal.EmailAddress,
                                                    Roles = new Roles{ Role.Participant.ToString() },
                                                }
                              };


            command.Browser.ExternalCredentials
                = new HashSet<BrowserIdentityProviderCredential>()
                      {
                          new BrowserIdentityProviderCredential()
                              {
                                  BrowserId = command.Browser.Id,
                                  IdentityProvider = principal.IdentityProvider,
                                  UserIdentifier = principal.NameIdentifier
                              }
                      };
            _messageBus.Send(command);
        }

        [Authorize]
        public ActionResult ManageTokens()
        {
            return View(_browserInformation.Browser.ExternalCredentials.ToList());
        }

        [Authorize]
        public ActionResult RequestToken(string providerIdentifier, string callbackAction, string callbackController, string ReturnUrl = null)
        {
            var identityProvider = SetUpIdentityProvider(providerIdentifier, "Account", "TokenResponse", ReturnUrl);
            identityProvider.CallbackUrl += "&callbackController=" + callbackController + "&callbackAction=" +
                                            callbackAction;
            if(string.IsNullOrWhiteSpace(ReturnUrl))
                identityProvider.CallbackUrl += "&ReturnUrl=" + ReturnUrl;

            if(Url != null)
                identityProvider.CallbackUrl = Url.Encode(identityProvider.CallbackUrl);
            identityProvider.RequestAuthorisation();
            return new EmptyResult();
        }

        protected IdentityProviderInterface SetUpIdentityProvider(string providerIdentifier, string callbackController, string callbackAction, string ReturnUrl)
        {
            var callback = GetUrlCallBack(providerIdentifier, callbackController, callbackAction, ReturnUrl);

            var realmUri = new Uri(callback);

            var identityProvider = _identityProviderService.GetProviderByIdentifier(providerIdentifier);
            identityProvider.CallbackUrl = callback;
            identityProvider.RealmUri = realmUri.Scheme + "://" + realmUri.Authority;
            return identityProvider;
        }

        public object TokenResponse(string providerIdentifier, string callbackController, string callbackAction, string ReturnUrl = null)
        {
            var identityProvider = SetUpIdentityProvider(providerIdentifier, "Account", "TokenResponse", ReturnUrl);
            identityProvider.CallbackUrl += "&callbackController=" + callbackController + "&callbackAction=" +
                                            callbackAction;
            if(string.IsNullOrWhiteSpace(ReturnUrl))
                identityProvider.CallbackUrl += "&ReturnUrl=" + ReturnUrl;

            if (Url != null)
                identityProvider.CallbackUrl = Url.Encode(identityProvider.CallbackUrl);
            var browserCreds = new BrowserIdentityProviderCredential()
            {
                BrowserId = _browserInformation.Browser.Id
            };
            browserCreds.CopyFieldsFrom(identityProvider.GetCredentials());

            var command = new SetExternalCredentialCommand()
            {
                Credential = browserCreds
            };

            _messageBus.Send(command);

            return RedirectToAction(callbackAction, callbackController, new { providerName = providerIdentifier, ReturnUrl });
        }
    }
}
