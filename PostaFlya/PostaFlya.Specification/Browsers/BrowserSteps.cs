﻿using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using NUnit.Framework;
using Ninject;
using TechTalk.SpecFlow;
using PostaFlya.Controllers;
using PostaFlya.Domain.Behaviour;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Authentication;
using PostaFlya.Models.Browser;
using PostaFlya.Models.Location;
using PostaFlya.Specification.DynamicBulletinBoard;
using PostaFlya.Specification.Fliers;
using PostaFlya.Specification.Util;
using Website.Infrastructure.Query;
using Website.Test.Common;
using Website.Domain.Browser;
using Roles = Website.Domain.Browser.Roles;

namespace PostaFlya.Specification.Browsers
{
    [Binding]
    public class BrowserSteps
    {
        [Given(@"i have navigated to the log on page")]
        public void GivenIHaveNavigatedToTheLogOnPage()
        {          
            var accountController = SpecUtil.GetController<AccountController>();
             accountController.LoginPage();
        }

        [Then(@"i will be Redirected to the Identity Providers Login Page")]
        public void ThenIWillBeRedirectedToTheIdentityProvidersLoginPage()
        {
            var result = SpecUtil.ControllerResult as EmptyResult;
            var httpResp = SpecUtil.CurrIocKernel.GetMock<HttpResponseBase>();
            httpResp.Verify(response => response.Redirect("www.google.com"));
        }

        [When(@"i provide Select an Identity Provider")]
        public void WhenIProvideSelectAnIdentityProvider()
        {
            
            var accountController = SpecUtil.GetController<AccountController>();
            ControllerContextMock.FakeControllerContext(SpecUtil.CurrIocKernel, accountController);
            SpecUtil.ControllerResult = accountController.AuthRequest("TestProvider");
        }

        [Given(@"i have recieve a resonse from an Identity Provider")]
        public void GivenIHaveRecieveAResonseFromAnIdentityProvider()
        {
            var accountController = SpecUtil.GetController<AccountController>();
            ControllerContextMock.FakeControllerContext(SpecUtil.CurrIocKernel, accountController);
            SpecUtil.ControllerResult = accountController.AuthResponse("TestProvider");
        }

        [Then(@"My credentials will be used to log me in")]
        public void ThenMyCredentialsWillBeUsedToLogMeIn()
        {
            var httpResp = SpecUtil.CurrIocKernel.GetMock<HttpResponseBase>();
            var authCookie = httpResp.Object.Cookies[FormsAuthentication.FormsCookieName];
            Assert.IsNotNull(authCookie);
        }


        [When(@"i provide correct CREDENTIALS")]
        public void WhenIProvideCorrectCredentials()
        {
            var accountController = SpecUtil.GetController<AccountController>();
            ControllerContextMock.FakeControllerContext(SpecUtil.CurrIocKernel, accountController);
            accountController.AuthResponse("TestProvider");

            //below just does what about does but stores browser id in scenario context
            var test = SpecUtil.CurrIocKernel.Get<IdentityProviderServiceInterface>();
            var prov = test.GetProviderByIdentifier("TestProvider");
            var cred = prov.GetCredentials();
            var browserAsParticipant = SpecUtil.CurrIocKernel.Get<QueryChannelInterface>()
                                               .Query(new FindBrowserByIdentityProviderQuery() {Credential = cred},
                                                      (Browser) null);

            ScenarioContext.Current["browserId"] = browserAsParticipant.Id;
        }

        [Given(@"i am not yet a BROWSER with PARTICIPANT ROLE")]
        public void GivenIAmNotYetAbrowserWithParticipantRole()
        {
            AssertBrowserInParticipantRole(false);
        }

        //[Then(@"i will be AUTHENTICATED with the site")]
        //public void ThenIWillBeAuthenticatedWithTheSite()
        //{
        //    var accountController = SpecUtil.GetController<AccountController>();
        //    var forms = SpecUtil.CurrIocKernel.Get<FormCollection>(ib => ib.Get<bool>("ACSGoogleFormCollection"));
        //    accountController.ControllerContext = new ControllerContext(SpecUtil.RequestContext<WebPrincipalInterface>(), accountController);
        //    accountController.Authenticate(forms);

        //    Assert.IsTrue(accountController.User.Identity.IsAuthenticated);
        //}

        [Then(@"a BROWSER with PARTICIPANT ROLE will be created for me")]
        public void ThenAnAccountWillBeCreatedForMe()
        {
            AssertBrowserInParticipantRole(true);
        }

        public BrowserInterface AssertBrowserInParticipantRole(bool exists)
        {
            var browserQuery = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();
            var creds = new IdentityProviderCredential()
                            {IdentityProvider = IdentityProviders.GOOGLE, UserIdentifier = "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1ho"};
            var browser = SpecUtil.CurrIocKernel.Get<QueryChannelInterface>()
                                               .Query(new FindBrowserByIdentityProviderQuery() { Credential = creds },
                                                      (Browser) null);
            if (exists)
            {
                Assert.IsTrue(browser != null);
                Assert.IsTrue(browser.HasRole(Role.Participant));
            }
            else
            {
                Assert.IsNull(browser);
            }
            return browser;
        }

        [Given(@"i am currently operating in a BROWSER with TEMPORARY ROLE")]
        public void GivenIAmCurrentlyOperatingInABrowserWithTemporaryRole()
        {
            ScenarioContext.Current["currbrowser"] = new Browser(Guid.NewGuid().ToString()) { Roles = new Roles { Role.Temporary.ToString()} };
        }

        [Then(@"my registered BROWSER will be loaded as the ACTIVE BROWSER")]
        public void ThenMyRegisteredBrowserWillBeLoadedAsTheActiveBrowser()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            Assert.IsTrue(browserInformation.Browser.HasRole(Role.Participant));
        }

        [Then(@"the profile details will be stored against my browser")]
        public void ThenThePersonalDetailsWillBeStoredAgainstMyBrowser()
        {
            var profileEditModel = ScenarioContext.Current["ProfileEditModel"] as ProfileEditModel;
            var browserInformation = SpecUtil.GetCurrBrowser();
            var myDetailsController = SpecUtil.GetApiController<MyDetailsController>();
            var browserRet = myDetailsController.Get(browserInformation.Browser.Id);
            Assert.AreEqual(profileEditModel.Handle, browserRet.Handle);
            Assert.AreEqual(profileEditModel.FirstName, browserRet.FirstName);
            Assert.AreEqual(profileEditModel.Surname, browserRet.Surname);
            Assert.AreEqual(profileEditModel.MiddleNames, browserRet.MiddleNames);
            Assert.AreEqual(profileEditModel.EmailAddress, browserRet.EmailAddress);
            Assert.AreEqual(profileEditModel.Address.ToDomainModel(), browserRet.Address.ToDomainModel());
            Assert.AreEqual(profileEditModel.AvatarImageId, browserRet.AvatarImageId);
            Assert.AreEqual(profileEditModel.WebSite, browserRet.WebSite);

        }

        //| Name | FirstName | MiddleNames | Surname  | Email | Address  | Avatar |
        [When(@"I update my profile details with the following data:")]
        public void WhenIUpdateMyProfileDetailsWithTheFollowingData(Table table)
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            var editModel = new ProfileEditModel()
                                {
                                    Id = browserInformation.Browser.Id,
                                    Handle = table.Rows[0]["Name"],
                                    Address = new LocationModel( new Website.Domain.Location.Location(table.Rows[0]["Address"])),
                                    AvatarImageId = table.Rows[0]["Avatar"],
                                    FirstName = table.Rows[0]["FirstName"],
                                    MiddleNames = table.Rows[0]["MiddleNames"],
                                    Surname = table.Rows[0]["Surname"],
                                    EmailAddress = table.Rows[0]["Email"],
                                    WebSite = table.Rows[0]["WebSite"]
                                };
            WhenIUpdateMyProfileDetails(editModel);
        }

        public void WhenIUpdateMyProfileDetails(ProfileEditModel editModel)
        {
            var myDetailsController = SpecUtil.GetApiController<MyDetailsController>();
            var res = myDetailsController.Put(editModel);
            res.AssertStatusCode();
            ScenarioContext.Current["ProfileEditModel"] = editModel;
        }


        [Then(@"i will see my posted fliers")]
        public void ThenIWillSeeMyFliers()
        {
            new ClaimSteps().GivenThereIsAflierABrowserHasClaimATearOffFor();
            new FlierSteps().GivenABrowserHasCreatedAFlierofBehaviour(FlierBehaviour.Default.ToString());

            var browserId = ScenarioContext.Current["existingbrowserid"] as string;
            var myFliersController = SpecUtil.GetApiController<MyFliersController>();
            var browser = SpecUtil.AssertGetParticipantBrowser(browserId);
            var ret = myFliersController.Get(browser.Id);
            
            Assert.IsNotNull(ret);         
            Assert.IsNotEmpty(ret);
            AssertUtil.AreAll(ret, model => model.BrowserId == browser.Id);
        }

        [Then(@"i will see my tear off claims")]
        public void ThenIWillSeeMyTearOffClaims()
        {
            new ClaimSteps().GivenThereIsAflierABrowserHasClaimATearOffFor();
            new FlierSteps().GivenABrowserHasCreatedAFlierofBehaviour(FlierBehaviour.Default.ToString());

            var browserId = ScenarioContext.Current["existingbrowserid"] as string;
            var profileController = SpecUtil.GetApiController<ClaimController>();
            var browser = SpecUtil.AssertGetParticipantBrowser(browserId);
            var ret = profileController.Get(browser.Id);

            Assert.IsNotNull(ret);
            Assert.IsNotEmpty(ret);
            AssertUtil.AreAll(ret, model => model.BrowserId != browser.Id);
        }

        [When(@"i navigate to my flyas")]
        public void WhenINavigateToMyFliers()
        {
            var browserId = ScenarioContext.Current["existingbrowserid"] as string;
            var profileController = SpecUtil.GetController<ProfileController>();
            var browser = SpecUtil.AssertGetParticipantBrowser(browserId);
            profileController.Posted();
        }

        [When(@"i navigate to my peeled flyas")]
        public void WhenINavigateToMyPeeledFliers()
        {
            var browserId = ScenarioContext.Current["existingbrowserid"] as string;
            var profileController = SpecUtil.GetController<ProfileController>();
            var browser = SpecUtil.AssertGetParticipantBrowser(browserId);
            profileController.Peeled();
        }

        

    }
}
