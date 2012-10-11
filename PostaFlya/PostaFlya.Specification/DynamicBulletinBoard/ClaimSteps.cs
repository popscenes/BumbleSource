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
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Specification.Fliers;
using PostaFlya.Specification.Util;
using Website.Domain.Claims;
using Website.Domain.Service;
using Website.Infrastructure.Query;

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
                SpecUtil.CurrIocKernel.Get<FlierQueryServiceInterface>().FindById<Flier>(flier.Id);
        }

        [Then(@"I will be recorded as having claimed the flier once")]
        public void ThenIWillBeRecordedAsHavingClaimedATearOff()
        {
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var queryService = SpecUtil.CurrIocKernel.Get<FlierQueryServiceInterface>();
            var browserInformation = SpecUtil.GetCurrBrowser();

            var claims = queryService.FindAggregateEntities<Claim>(flier.Id);
            Assert.IsTrue(claims.SingleOrDefault(l => l.AggregateId == flier.Id && l.BrowserId == browserInformation.Browser.Id) != null);
        }

        [Then(@"the FLIER tear off claims will remain the same")]
        public void ThenTheFlierTearOffsWillRemainTheSame()
        {
            var initClaims = (int)ScenarioContext.Current["initialclaims"];
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var queryService = SpecUtil.CurrIocKernel.Get<FlierQueryServiceInterface>();
            var retFlier = queryService.FindById<Flier>(flier.Id);

            Assert.AreEqual(initClaims, retFlier.NumberOfClaims);
        }

        [Then(@"the number of claims against the FLIER will be incremented")]
        public void ThenTheFlierClaimsWillBeIncremented()
        {
            var initClaims = (int)ScenarioContext.Current["initialclaims"];
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var queryService = SpecUtil.CurrIocKernel.Get<FlierQueryServiceInterface>();
            var retFlier = queryService.FindById<Flier>(flier.Id);

            Assert.AreEqual(initClaims + 1, retFlier.NumberOfClaims);
        }

        [Then(@"I should see the claimed tear offs for the FLIER")]
        public void ThenIShouldSeeTheClaimedTearOffsForTheFlier()
        {
            var viewMod = ScenarioContext.Current["fliermodel"] as DefaultDetailsViewModel;
            var createClaimModel = ScenarioContext.Current["CreateClaimModel"] as CreateClaimModel;
            var claimController = SpecUtil.GetApiController<ClaimController>();
            var ret = claimController.Get(EntityTypeEnum.Flier, viewMod.Flier.Id);
            Assert.IsNotNull(ret);
            Assert.IsNotEmpty(ret);
            Assert.IsTrue(ret.Any(c => c.Browser.Id == createClaimModel.BrowserId));

        }


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

        [BeforeScenario("TearOffNotification")]
        public void SetupNotificationBinding()
        {
            var claimBind = SpecUtil.CurrIocKernel.GetMock<PublicationServiceInterface>();
            claimBind.Setup(service => service.Publish(It.IsAny<Claim>()))
                .Callback<Claim>(claim => 
                    ScenarioContext.Current["tearoffnotification"] = true);
            SpecUtil.CurrIocKernel.Bind<PublicationServiceInterface>()
                .ToConstant(claimBind.Object);
        }

        [AfterScenario("TearOffNotification")]
        public void TearDownNotificationBinding()
        {
            SpecUtil.CurrIocKernel.Unbind<PublicationServiceInterface>();
        }
    }
}
