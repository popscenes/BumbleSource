using System;
using System.Net.Http;
using NUnit.Framework;
using Ninject;
using PostaFlya.Domain.Flier;
using TechTalk.SpecFlow;
using PostaFlya.Controllers;
using Website.Domain.Browser;
using Website.Domain.Browser.Command;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using PostaFlya.Specification.Browsers;
using PostaFlya.Specification.Util;
using Website.Test.Common;

namespace PostaFlya.Specification
{
    [Binding]
    public class CommonSteps
    {
        private readonly BrowserSteps _browserSteps = new BrowserSteps();
        [Then(@"I should observe the failure ""(.*)""")]
        public void IShouldObserveTheApiFailure(string errorMessage)
        {
            var res = ScenarioContext.Current["responsemessage"]  as HttpResponseMessage;
            var msg = res.AssertStatusCodeFailed();
            Assert.IsNotNull(msg);
            Assert.AreEqual(errorMessage, msg.Message);
        }

        
        [Given(@"i am an existing BROWSER with PARTICIPANT ROLE")]
        public string GivenIAmAnExistingBrowserWithParticipantRole()
        {
            var accountController = SpecUtil.GetController<AccountController>();
            ControllerContextMock.FakeControllerContext(SpecUtil.CurrIocKernel, accountController);
            var res = accountController.CreateBrowserFromIdentityProviderCredentials(new IdentityProviderCredential() { IdentityProvider = IdentityProviders.GOOGLE, UserIdentifier = "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1ho" })
                as MsgResponse;

            
            _browserSteps.AssertBrowserInParticipantRole(true);
            Assert.IsNotNull(res);
            Assert.IsFalse(res.IsError);
            var browserId = res.GetEntityId();
            Assert.IsFalse(string.IsNullOrWhiteSpace(browserId));
            ScenarioContext.Current["existingbrowserid"] = browserId;
            return browserId;
        }

        [Given(@"There is an existing BROWSER with PARTICIPANT ROLE")]
        public string GivenThereIsAnExistingBrowserWithParticipantRole()
        {
            if(ScenarioContext.Current.ContainsKey("existingbrowserid"))
            {
                return ScenarioContext.Current["existingbrowserid"] as string;
            }

            var accountController = SpecUtil.GetController<AccountController>();
            ControllerContextMock.FakeControllerContext(SpecUtil.CurrIocKernel, accountController);
            var res = accountController.CreateBrowserFromIdentityProviderCredentials(new IdentityProviderCredential() 
            { IdentityProvider = IdentityProviders.GOOGLE, UserIdentifier = Guid.NewGuid().ToString() })
                as MsgResponse;
            Assert.IsNotNull(res);
            Assert.IsFalse(res.IsError);
            var browserId = res.GetEntityId();
            Assert.IsFalse(string.IsNullOrWhiteSpace(browserId));
            ScenarioContext.Current["existingbrowserid"] = browserId;
            return browserId;
        }

        //REUSE
        [Given(@"I am a BROWSER in PARTICIPANT ROLE")]
        public void GivenIamABrowserInParticipantRole()
        {
            GivenIAmAnExistingBrowserWithParticipantRole();
            _browserSteps.WhenIProvideCorrectCredentials();
            _browserSteps.ThenMyRegisteredBrowserWillBeLoadedAsTheActiveBrowser();
        }

        //REUSE
        [Given(@"I am a PARTICIPANT with (.*) ROLE")]
        public void GivenIamAParticipantWithRole(string role)
        {
            GivenIAmAnExistingBrowserWithParticipantRole();
            _browserSteps.WhenIProvideCorrectCredentials();
            _browserSteps.ThenMyRegisteredBrowserWillBeLoadedAsTheActiveBrowser();
            GivenIHaveRole(role);
        }

        [Given(@"I have (.*) ROLE")]
        public void GivenIHaveRole(string role)
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            if (!browserInformation.Browser.Roles.Contains(role))
            {
                browserInformation.Browser.Roles.Add(role);
                SpecUtil.CurrIocKernel.Get<GenericRepositoryInterface>()
                    .UpdateEntity<Browser>(
                        browserInformation.Browser.Id, browser => browser.Roles.Add(role));
            }
        }

        //REUSE
        [Given(@"I am a PARTICIPANT without (.*) ROLE")]
        public void GivenIamAParticipantWithoutRole(string role)
        {
            GivenIAmAnExistingBrowserWithParticipantRole();
            _browserSteps.WhenIProvideCorrectCredentials();
            _browserSteps.ThenMyRegisteredBrowserWillBeLoadedAsTheActiveBrowser();
            var browserInformation = SpecUtil.GetCurrBrowser();
            if (browserInformation.Browser.Roles.Contains(role))
            {
                browserInformation.Browser.Roles.Remove(role);

                SpecUtil.CurrIocKernel.Get<GenericRepositoryInterface>()
                .UpdateEntity<Browser>(
                    browserInformation.Browser.Id, browser => browser.Roles.Remove(role));
            }
        }

        //REUSE
        [Then(@"I will have (.*) ROLE")]
        public void ThenIWillHaveRole(string role)
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            CollectionAssert.Contains(browserInformation.Browser.Roles, role);
        }

        public void GivenABrowserHasAccountCredit(string browserId, int credit)
        {
            var browserReo = SpecUtil.CurrIocKernel.Get<GenericRepositoryInterface>();
            browserReo.UpdateEntity<Browser>(browserId,
                    b =>
                    {
                        b.AccountCredit = credit;
                    });
            ScenarioContext.Current["initialcredit"] = credit;
        }

        [Given(@"I have (.*) Account Credits")]
        public void GivenIHaveAccountCredit(int credit)
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            GivenABrowserHasAccountCredit(browserInformation.Browser.Id, credit);
        }

        [Given(@"The Flier Creator Has (.*) Account Credits")]
        public void GivenTheFlierCreatorHasAccountCredit(int credit)
        {
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            GivenABrowserHasAccountCredit(flier.BrowserId, credit);
        }

    }
}
