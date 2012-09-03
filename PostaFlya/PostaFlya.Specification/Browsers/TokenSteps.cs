using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using MbUnit.Framework;
using PostaFlya.Controllers;
using PostaFlya.Specification.Util;
using TechTalk.SpecFlow;
using WebSite.Infrastructure.Authentication;
using Website.Domain.Browser;

namespace PostaFlya.Specification.Browsers
{
    [Binding]
    public class TokenSteps
    {
        [Then(@"Then i will be shown a list of my external TOKENS and thier current status")]
        public void ThenThenIWillBeShownAListOfMyExternalTOKENSAndThierCurrentStatus()
        {
            var result = SpecUtil.ControllerResult as ViewResult;
            var credentials = result.Model as List<BrowserIdentityProviderCredential>;
            Assert.Count(1, credentials);
            Assert.AreEqual(credentials[0].IdentityProvider, "google");
        }

        [When(@"I go to the TOKEN MANAGMENT SCREEN")]
        public void WhenIGoToTheTOKENMANAGMENTSCREEN()
        {
            var accountController = SpecUtil.GetController<AccountController>();
            SpecUtil.ControllerResult = accountController.ManageTokens();

        }

        [When(@"I request an access token for a source")]
        public void WhenIRequestAnAccessTokenForASource()
        {
            var accountController = SpecUtil.GetController<AccountController>();
            accountController.RequestToken("facebook", "ManageTokens", "Account");
        }

        [Then(@"Then i will be redirected to obtain a valid token")]
        public void ThenThenIWillBePromptedToObtainAValidToken()
        {
            var httpResp = SpecUtil.CurrIocKernel.GetMock<HttpResponseBase>();
            httpResp.Verify(response => response.Redirect("https://graph.facebook.com/oauth/access_token"));
        }

        


        [When(@"I obtain a valid token from the source")]
        public void WhenIObtainAValidTokenFromTheSource()
        {
            var accountController = SpecUtil.GetController<AccountController>();
            SpecUtil.ControllerResult = accountController.TokenResponse(IdentityProviders.FACEBOOK, "Account", "ManageTokens");
        }

        [Then(@"Then i will be redircted back to the redirect page specified")]
        public void ThenThenIWillBeRedirctedBackToTheFlierImportPage()
        {
            var result = SpecUtil.ControllerResult as RedirectToRouteResult;
            Assert.AreEqual(result.RouteValues["controller"], "Account");
            Assert.AreEqual(result.RouteValues["action"], "ManageTokens");

        }

        [Then(@"I will have a valid access token for the source")]
        public void ThenIWillHaveAValidAccessTokenForTheSource()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            var browser = browserInformation.Browser as Browser;

            var browserIdentityProviderCredential = browserInformation.Browser.ExternalCredentials.FirstOrDefault(_ => _.IdentityProvider == IdentityProviders.FACEBOOK);
            if (browserIdentityProviderCredential != null)
            {
                var tokenExpires = browserIdentityProviderCredential.AccessToken.Expires;

                Assert.IsTrue(DateTime.Now < tokenExpires);
            }
        }

    }
}
