using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Ninject;
using PostaFlya.Models.Claims;
using TechTalk.SpecFlow;
using PostaFlya.Areas.Default.Models;
using PostaFlya.Binding;
using PostaFlya.Controllers;
using PostaFlya.Domain.Flier;
using PostaFlya.Specification.Fliers;
using PostaFlya.Specification.Util;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;
using Website.Domain.Claims;
using Website.Domain.Claims.Event;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;
using Website.Mocks.Domain.Data;

namespace PostaFlya.Specification.DynamicBulletinBoard
{
    [Binding]
    public class ClaimSteps
    {
        private readonly CommonSteps _common = new CommonSteps();
        [When(@"I claim a tear off for that FLIER")]
        public void WhenIClaimATearOffForThatFlier()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            WhenABrowserClaimsATearOffForThatFlier(browserInformation.Browser.Id);
        }

        [When(@"Another Browser claims a tear off for that FLIER")]
        public void WhenAnotherBrowserClaimsATearOffForThatFLIER()
        {
            var newBrowser = BrowserTestData.GetOne(SpecUtil.CurrIocKernel);

            BrowserTestData.StoreOne(newBrowser, SpecUtil.CurrIocKernel.Get<GenericRepositoryInterface>(), SpecUtil.CurrIocKernel);
            WhenABrowserClaimsATearOffForThatFlier(newBrowser.Id);
        }


        public void WhenABrowserClaimsATearOffForThatFlier(string browserId)
        {
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var claimController = SpecUtil.GetApiController<ClaimController>();
            var claim = new CreateClaimModel()
            {
                ClaimEntity = EntityTypeEnum.Flier,
                EntityId = flier.Id,
                BrowserId = browserId,
            };

            ScenarioContext.Current["initialclaims"] = flier.NumberOfClaims;
            ScenarioContext.Current["CreateClaimModel"] = claim;
            var res = claimController.Post(claim);
            res.EnsureSuccessStatusCode();
        }
        
        [Given(@"I have already claimed a tear off for that FLIER")]
        [Given(@"Someone has claimed a tear off for a FLIER")]
        public void GivenIHaveClaimedATearOffForAFlier()
        {
            WhenIClaimATearOffForThatFlier();
            ThenIWillBeRecordedAsHavingClaimedATearOff();
            //reload the new version of the flier
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            ScenarioContext.Current["flier"] =
                SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>().FindById<Flier>(flier.Id);
        }

        [Then(@"I will be recorded as having claimed the flier once")]
        public void ThenIWillBeRecordedAsHavingClaimedATearOff()
        {
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var queryService = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();
            var browserInformation = SpecUtil.GetCurrBrowser();

            var claims = queryService.FindAggregateEntities<Claim>(flier.Id);
            Assert.IsTrue(claims.SingleOrDefault(l => l.AggregateId == flier.Id && l.BrowserId == browserInformation.Browser.Id) != null);
        }

        [Then(@"the FLIER tear off claims will remain the same")]
        public void ThenTheFlierTearOffsWillRemainTheSame()
        {
            var initClaims = (int)ScenarioContext.Current["initialclaims"];
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var queryService = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();
            var retFlier = queryService.FindById<Flier>(flier.Id);

            Assert.AreEqual(initClaims, retFlier.NumberOfClaims);
        }

        [Then(@"the number of claims against the FLIER will be incremented")]
        public void ThenTheFlierClaimsWillBeIncremented()
        {
            var initClaims = (int)ScenarioContext.Current["initialclaims"];
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var queryService = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();
            var retFlier = queryService.FindById<Flier>(flier.Id);

            Assert.AreEqual(initClaims + 1, retFlier.NumberOfClaims);
        }

//        [Then(@"I should see the claimed tear offs for the FLIER")]
//        public void ThenIShouldSeeTheClaimedTearOffsForTheFlier()
//        {
//            var viewMod = ScenarioContext.Current["fliermodel"] as DefaultDetailsViewModel;
//            var createClaimModel = ScenarioContext.Current["CreateClaimModel"] as CreateClaimModel;
//            var claimController = SpecUtil.GetApiController<ClaimController>();
//            var ret = claimController.Get(EntityTypeEnum.Flier, viewMod.Flier.Id);
//            Assert.IsNotNull(ret);
//            Assert.IsNotEmpty(ret);
//            
//            //Assert.IsTrue(ret.Any(c => c.Browser.Id == createClaimModel.BrowserId));
//
//        }


        [Given(@"There is a FLIER Someone has claimed a tear off for a FLIER")]
        public void GivenThereIsAflierABrowserHasClaimATearOffFor()
        {
            new FlierSteps().GivenIHaveCreatedAflier();
            _common.GivenThereIsAnExistingBrowserWithParticipantRole();
            var browserId = ScenarioContext.Current["existingbrowserid"] as string;
            WhenABrowserClaimsATearOffForThatFlier(browserId);
        }

        [Then(@"A Notification for that Tear Off should be published")]
        public void ThenANotificationForThatTearOffShouldBePublished()
        {
            var test = ScenarioContext.Current["tearoffnotification"];
            Assert.IsNotNull(test);
        }
        [Then(@"(.*) will be deducted from the Flier Creators Account")]
        public void ThenWillBeDeductedFromTheFlierCreatorsAccount(int deduction)
        {
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var queryService = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();
            var retFlier = queryService.FindById<Flier>(flier.Id);

            var browser = queryService.FindById<Browser>(retFlier.BrowserId);

            var initcredit = (int)ScenarioContext.Current["initialcredit"];

            Assert.AreEqual(browser.AccountCredit, initcredit - deduction);
        }

        

//        [When(@"I claim a tear off for that FLIER and send my contact details")]
//        public void WhenIClaimATearOffForThatFLIERAndSendMyContactDetails()
//        {
//            var flier = ScenarioContext.Current["flier"] as FlierInterface;
//            var claimController = SpecUtil.GetApiController<ClaimController>();
//            var browserInformation = SpecUtil.GetCurrBrowser();
//
//            var claim = new CreateClaimModel()
//            {
//                ClaimEntity = EntityTypeEnum.Flier,
//                EntityId = flier.Id,
//                BrowserId = browserInformation.Browser.Id,
//                //SendClaimerContactDetails = true,
//                ClaimerMessage = "Fix My Plumbing!"
//            };
//
//            ScenarioContext.Current["initialclaims"] = flier.NumberOfClaims;
//            ScenarioContext.Current["CreateClaimModel"] = claim;
//            var res = claimController.Post(claim);
//            res.EnsureSuccessStatusCode();
//        }

//        [Then(@"the Claim will be recorded as having My Contact Details")]
//        public void ThenTheClaimWillBeRecordedAsHavingMyContactDetails()
//        {
//            var flier = ScenarioContext.Current["flier"] as FlierInterface;
//            var queryService = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();
//            var browserInformation = SpecUtil.GetCurrBrowser();
//
//            var claims = queryService.FindAggregateEntities<Claim>(flier.Id);
//
//            var claim = claims.First();
//            Assert.AreEqual(claim.ClaimContext, FlierInterfaceExtensions.ClaimContextSendUserDetailsEnabled);
//            Assert.AreEqual(claim.ClaimMessage, "Fix My Plumbing!");
//        }

        [Then(@"the number of claims against the FLIER will not be incremented")]
        public void ThenTheNumberOfClaimsAgainstTheFLIERWillNotBeIncremented()
        {
            var initClaims = (int)ScenarioContext.Current["initialclaims"];
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var queryService = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();
            var retFlier = queryService.FindById<Flier>(flier.Id);

            Assert.AreEqual(initClaims, retFlier.NumberOfClaims);
        }


        [BeforeScenario("TearOffNotification")]
        public void SetupNotificationBinding()
        {
            var claimBind = SpecUtil.CurrIocKernel.GetMock<DomainEventPublishServiceInterface>();
            claimBind.Setup(service => service.Publish(It.IsAny<ClaimEvent>()))
                .Callback<ClaimEvent>(claim => 
                    ScenarioContext.Current["tearoffnotification"] = true);
            SpecUtil.CurrIocKernel.Bind<DomainEventPublishServiceInterface>()
                .ToConstant(claimBind.Object);
        }

        [AfterScenario("TearOffNotification")]
        public void TearDownNotificationBinding()
        {
            SpecUtil.CurrIocKernel.Unbind<DomainEventPublishServiceInterface>();
        }

        [Then(@"I should see the Contact Details associated with that FLIER")]
        public void ThenIShouldSeeTheContactDetailsAssociatedWithThatFLIER()
        {
            var mod = ScenarioContext.Current["fliermodel"]  as DefaultDetailsViewModel;
            Assert.That(mod.ContactDetails, Is.Not.Null);
        }


    }
}
