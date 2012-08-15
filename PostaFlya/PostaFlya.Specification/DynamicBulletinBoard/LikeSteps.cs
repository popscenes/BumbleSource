using System.Linq;
using MbUnit.Framework;
using Ninject;
using TechTalk.SpecFlow;
using PostaFlya.Areas.Default.Models;
using PostaFlya.Binding;
using PostaFlya.Controllers;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Models.Likes;
using PostaFlya.Specification.Browsers;
using PostaFlya.Specification.Fliers;
using PostaFlya.Specification.Util;

namespace PostaFlya.Specification.DynamicBulletinBoard
{
    [Binding]
    public class LikeSteps
    {
        private readonly CommonSteps _common = new CommonSteps();
        [When(@"I like that FLIER")]
        public void WhenILikeThatFlier()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            WhenABrowserLikesThatFlier(browserInformation.Browser.Id);
        }

        public void WhenABrowserLikesThatFlier(string browserId)
        {
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var likeController = SpecUtil.GetApiController<LikeController>();
            var like = new CreateLikeModel()
            {
                LikeEntity = EntityTypeEnum.Flier,
                EntityId = flier.Id,
                BrowserId = browserId,
                Comment = "BumbleFlya is ace!"
            };

            ScenarioContext.Current["initiallikes"] = flier.NumberOfLikes;
            ScenarioContext.Current["CreateLikeModel"] = like;
            var res = likeController.Post(like);
            res.EnsureSuccessStatusCode();
        }

        [Given(@"I have liked a FLIER")]
        [Given(@"Someone has liked a FLIER")]
        public void GivenIHaveLikedAflier()
        {
            WhenILikeThatFlier();
            ThenIWillBeRecordedAsHavingLikedTheFlierOnce();
            //reload the new version of the flier
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            ScenarioContext.Current["flier"] =
                SpecUtil.CurrIocKernel.Get<FlierQueryServiceInterface>().FindById(flier.Id);
        }

        [Then(@"I will be recorded as having liked the flier once")]
        public void ThenIWillBeRecordedAsHavingLikedTheFlierOnce()
        {
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var queryService = SpecUtil.CurrIocKernel.Get<FlierQueryServiceInterface>();
            var browserInformation = SpecUtil.GetCurrBrowser();

            var likes = queryService.GetLikes(flier.Id);
            Assert.IsTrue(likes.SingleOrDefault(l => l.EntityId == flier.Id && l.BrowserId == browserInformation.Browser.Id) != null);
        }

        [Then(@"the FLIER likes will remain the same")]
        public void ThenTheFlierLikesWillRemainTheSame()
        {
            var initLikes = (int)ScenarioContext.Current["initiallikes"];
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var queryService = SpecUtil.CurrIocKernel.Get<FlierQueryServiceInterface>();
            var retFlier = queryService.FindById(flier.Id);

            Assert.AreEqual(initLikes, retFlier.NumberOfLikes);
        }

        [Then(@"the FLIER likes will be incremented")]
        public void ThenTheFlierLikesWillBeIncremented()
        {
            var initLikes = (int)ScenarioContext.Current["initiallikes"];
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var queryService = SpecUtil.CurrIocKernel.Get<FlierQueryServiceInterface>();
            var retFlier = queryService.FindById(flier.Id);

            Assert.AreEqual(initLikes + 1, retFlier.NumberOfLikes);
        }

        [Then(@"I should see the likes for the FLIER")]
        public void ThenIShouldSeeTheCommentsForTheFlier()
        {
            var viewMod = ScenarioContext.Current["fliermodel"] as DefaultDetailsViewModel;
            var createLikeModel = ScenarioContext.Current["CreateLikeModel"] as CreateLikeModel;
            var likeController = SpecUtil.GetApiController<LikeController>();
            var ret = likeController.Get(EntityTypeEnum.Flier, viewMod.Flier.Id);
            Assert.IsNotNull(ret);
            Assert.IsNotEmpty(ret);
            Assert.IsTrue(ret.Any(c => c.Browser.Id == createLikeModel.BrowserId));

        }


        [Given(@"There is a FLIER Someone has liked a FLIER")]
        public void GivenThereIsAflierABrowserHasLiked()
        {
            new FlierSteps().GivenIHaveCreatedAflier();
            _common.GivenThereIsAnExistingBrowserWithParticipantRole();
            var browserId = ScenarioContext.Current["existingbrowserid"] as string;
            WhenABrowserLikesThatFlier(browserId);
        }
    }
}
