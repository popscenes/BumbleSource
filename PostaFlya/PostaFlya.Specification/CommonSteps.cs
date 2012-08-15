using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using MbUnit.Framework;
using TechTalk.SpecFlow;
using PostaFlya.Controllers;
using WebSite.Infrastructure.Authentication;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;
using PostaFlya.Specification.Browsers;
using PostaFlya.Specification.Util;
using WebSite.Test.Common;

namespace PostaFlya.Specification
{
    [Binding]
    public class CommonSteps
    {
        private readonly BrowserSteps _browserSteps = new BrowserSteps();
        [Then(@"I should observe the failure ""(.*)""")]
        public void GivenIHaveEnteredSomethingIntoTheCalculator(string errorMessage)
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
            var browserInformation = SpecUtil.GetCurrBrowser();
            if (!browserInformation.Browser.Roles.Contains(role))
                browserInformation.Browser.Roles.Add(role);
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
                browserInformation.Browser.Roles.Remove(role);
        }

        //REUSE
        [Then(@"I will have (.*) ROLE")]
        public void ThenIWillHaveRole(string role)
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            Assert.Contains(browserInformation.Browser.Roles, role);
        }
    }
}
