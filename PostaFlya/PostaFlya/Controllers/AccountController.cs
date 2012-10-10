﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Website.Application.Authentication;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Command;
using PostaFlya.Models;
using Website.Application.Domain.Browser;
using Website.Domain.Browser;
using Website.Domain.Browser.Command;
using Website.Domain.Browser.Query;
using Website.Domain.Location;

namespace PostaFlya.Controllers
{

    
    public class AccountController : Controller
    {
        private readonly IdentityProviderServiceInterface _identityProviderService;
        private readonly BrowserQueryServiceInterface _browserRepository;
        private readonly CommandBusInterface _commandBus;
        private readonly BrowserInformationInterface _browserInformation;

        public AccountController(IdentityProviderServiceInterface identityProviderService, BrowserQueryServiceInterface browserRepository,
            CommandBusInterface commandBus, BrowserInformationInterface browserInformation)
        {
            _identityProviderService = identityProviderService;
            _browserRepository = browserRepository;
            _commandBus = commandBus;
            _browserInformation = browserInformation;
        }

        public ActionResult LoginPage()
        {
            return View("Login");
        }

        [Authorize]
        public ActionResult AddBrowser(Browser browser)
        {
            var command = new AddBrowserCommand()
            {
               Browser = browser
            };

            _commandBus.Send(command);
            return View("Home");
        }


        protected string GetUrlCallBack(string providerIdentifier, string controller, string action)
        {
            var callback = "http://localhost/";
            if (Url != null)
                callback = Url.Action(action, controller, new { providerIdentifier = providerIdentifier }, "http");

                callback = callback.Replace("82", "81");
                callback = callback.Replace("83", "81");

            return callback;
        }

        public ActionResult AuthRequest(string providerIdentifier)
        {
            var identityProvider = SetUpIdentityProvider(providerIdentifier, "Account", "AuthResponse");
            identityProvider.RequestAuthorisation();
            return new EmptyResult();

        }

        [ValidateInput(false)]
        public ActionResult AuthResponse(string providerIdentifier)
        {
            var identityProvider = _identityProviderService.GetProviderByIdentifier(providerIdentifier);
            var callback = GetUrlCallBack(providerIdentifier, "Account", "AuthResponse");
            identityProvider.CallbackUrl = callback;
            var identityProviderCredentials = identityProvider.GetCredentials();
            SetBrowserFromIdentityProviderCredentials(identityProviderCredentials);
            CreateFormsCookieFromIdenityProviderCredentials(identityProviderCredentials);


            return RedirectToAction("Get", "Bulletin");
        }

        public void CreateFormsCookieFromIdenityProviderCredentials(IdentityProviderCredential identityProviderCredentials)
        {
            FormsAuthenticationTicket authTicket = new
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
            var browserAsParticipant = _browserRepository.FindByIdentityProvider(identityProviderCredentials);

            if (browserAsParticipant == null)
            {
                CreateBrowserFromIdentityProviderCredentials(identityProviderCredentials);
                browserAsParticipant = _browserRepository.FindByIdentityProvider(identityProviderCredentials);
            }

            //just set the current browser as the browser
            if (!browserAsParticipant.Equals(_browserInformation.Browser))
                _browserInformation.Browser = browserAsParticipant;


        }

        [NonAction]
        [Authorize]
        public object CreateBrowserFromIdentityProviderCredentials(IdentityProviderCredential identityProviderCredentials)
        {
            var command = new AddBrowserCommand()
            {
                Browser = new Website.Domain.Browser.Browser(Guid.NewGuid().ToString())
                {
                    Handle = identityProviderCredentials.Name,
                    EmailAddress = identityProviderCredentials.Email,
                    Roles = new Website.Domain.Browser.Roles { Role.Participant.ToString() },
                    SavedLocations = new Locations(),
                }
            };

            var creds = new BrowserIdentityProviderCredential(){BrowserId = command.Browser.Id};
            creds.CopyFieldsFrom(identityProviderCredentials);
            command.Browser.ExternalCredentials = new HashSet<BrowserIdentityProviderCredential>(){creds};

            return _commandBus.Send(command);
        }

        [Authorize]
        [ValidateInput(false)]
        public ActionResult Authenticate(FormCollection formCollection)
        {
            var principal = ((AzureWebPrincipal)User);
            var browserAsParticipant = _browserRepository.FindByIdentityProvider(principal.ToCredential());
                
            if (browserAsParticipant == null)
            {
                CreateBrowser(principal);
                browserAsParticipant = _browserRepository.FindByIdentityProvider(principal.ToCredential());
            }

            //just set the current browser as the browser
            if (!browserAsParticipant.Equals(_browserInformation.Browser))
                _browserInformation.Browser = browserAsParticipant;

            return RedirectToAction("Get", "Bulletin");
        }

        [NonAction]
        [Authorize]
        public void CreateBrowser(AzureWebPrincipal principal)
        {
            var command = new AddBrowserCommand()
                              {
                                  Browser = new Website.Domain.Browser.Browser(Guid.NewGuid().ToString())
                                                {
                                                    Handle = principal.Name,
                                                    EmailAddress = principal.EmailAddress,
                                                    Roles = new Website.Domain.Browser.Roles{ Role.Participant.ToString() },
                                                    SavedLocations = new Locations(),
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
            _commandBus.Send(command);
        }

        [Authorize]
        public ActionResult ManageTokens()
        {
            return View(_browserInformation.Browser.ExternalCredentials.ToList());
        }

        [Authorize]
        public ActionResult RequestToken(string providerIdentifier, string callbackAction, string callbackController)
        {
            var identityProvider = SetUpIdentityProvider(providerIdentifier, "Account", "TokenResponse");
            identityProvider.CallbackUrl += "&callbackController=" + callbackController + "&callbackAction=" + callbackAction;
            if(Url != null)
                identityProvider.CallbackUrl = Url.Encode(identityProvider.CallbackUrl);
            identityProvider.RequestAuthorisation();
            return new EmptyResult();
        }

        protected IdentityProviderInterface SetUpIdentityProvider(string providerIdentifier, string callbackController, string callbackAction)
        {
            var callback = GetUrlCallBack(providerIdentifier, callbackController, callbackAction);

            var realmUri = new Uri(callback);

            var identityProvider = _identityProviderService.GetProviderByIdentifier(providerIdentifier);
            identityProvider.CallbackUrl = callback;
            identityProvider.RealmUri = realmUri.Scheme + "://" + realmUri.Authority;
            return identityProvider;
        }

        public object TokenResponse(string providerIdentifier, string callbackController, string callbackAction)
        {
            var identityProvider = SetUpIdentityProvider(providerIdentifier, "Account", "TokenResponse");
            identityProvider.CallbackUrl += "&callbackController=" + callbackController + "&callbackAction=" + callbackAction;
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

            _commandBus.Send(command);

            return RedirectToAction(callbackAction, callbackController, new { providerName = providerIdentifier });
        }
    }
}
