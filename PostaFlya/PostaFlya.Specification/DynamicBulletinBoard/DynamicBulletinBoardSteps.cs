using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Ninject;
using TechTalk.SpecFlow;
using PostaFlya.Controllers;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Factory;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using PostaFlya.Models.Tags;
using PostaFlya.Specification.Fliers;
using PostaFlya.Specification.Util;
using Website.Domain.Location;
using Website.Domain.Tag;

namespace PostaFlya.Specification.DynamicBulletinBoard
{
    [Binding]
    public class DynamicBulletinBoardSteps
    {
        [Given(@"I have navigated to the BULLETIN BOARD for a LOCATION")]
        [When(@"I have navigated to the BULLETIN BOARD for a LOCATION")]
        public void GivenIHaveNavigatedToTheBulletinboardForALocation()
        {
            var bulletinController = SpecUtil.GetApiController<BulletinApiController>();
            var location = SpecUtil.CurrIocKernel.Get<Location>(ib => ib.Get<bool>("default"));
            var browserInfoService = SpecUtil.GetCurrBrowser();
            
            SpecUtil.ControllerResult = bulletinController
                .Get(location.ToViewModel(), 30, "", 0
                , browserInfoService.Browser.Distance ?? 0
                , browserInfoService.Browser.Tags.ToString());
        }



        //REUSE
        [Given(@"there are some TAGS set")]
        public void GivenThereAreSomeTagsSet()
        {
            SetSomeTagsSet();
        }

        public void SetSomeTagsSet(string testSetName = "default")
        {
            var settingController = SpecUtil.GetController<BrowserController>();
            var defaultTags = SpecUtil.CurrIocKernel.Get<Tags>(ib => ib.Get<bool>(testSetName));
            settingController.AddTags(new AddTagsModel() { TagsString = defaultTags.ToString() });
        }

        [Then(@"I should only see FLIERS within a DISTANCE from that LOCATION")]
        public void ThenIShouldSeeFliersWithinADistanceFromThatLocation()
        {
            var fliers = SpecUtil.ControllerResult as IList<BulletinFlierModel>;
            Assert.IsNotNull(fliers, "no fliers in context");

            var locService = SpecUtil.CurrIocKernel.Get<LocationServiceInterface>();
            foreach (var flier in fliers)
            {
                Assert.IsTrue(locService.IsWithinDefaultDistance(flier.Location.ToDomainModel()),
                              "Flier returned that isn't within default distance");
            }

            Assert.That(fliers.Count(), Is.EqualTo(3));
        }

        [Then(@"I should only see FLIERS with matching TAGS")]
        public void ThenIShouldSeeFliersWithMatchingTags()
        {
            var fliers = SpecUtil.ControllerResult as IList<BulletinFlierModel>;
            Assert.IsNotNull(fliers, "no fliers in context");

            var defaultTags = SpecUtil.CurrIocKernel.Get<Tags>(ib => ib.Get<bool>("default"));
            foreach (var flier in fliers)
            {
                Assert.IsTrue(defaultTags.Intersect(new Tags(flier.TagsString)).Any(),
                              "Flier returned that doesn't match tags %s", flier);
            }

            Assert.That(fliers.Count(), Is.EqualTo(3));
        }

        [When(@"I set my DISTANCE")]
        public void WhenISetMyDISTANCE()
        {
            var settingController = SpecUtil.GetController<BrowserController>();
            var browserInfoService = SpecUtil.GetCurrBrowser();
            Assert.IsTrue(!browserInfoService.Browser.Distance.HasValue);
            
            settingController.SetDistance(15);

            Assert.IsTrue(browserInfoService.Browser.Distance.HasValue);
        }

        [Then(@"i should see all fliers within my new DISTANCE that have matching TAGS")]
        public void ThenIShouldSeeAllFliersWithinMyNewDISTANCEThatHaveMatchingTAGS()
        {
            var bulletinApiController = SpecUtil.GetController<BulletinApiController>();
            var location = SpecUtil.CurrIocKernel.Get<Location>(ib => ib.Get<bool>("default"));
            var browserInfoService = SpecUtil.GetCurrBrowser();
            var result = bulletinApiController.Get(location.ToViewModel(), 30, "", 0
                , browserInfoService.Browser.Distance.GetValueOrDefault(), browserInfoService.Browser.Tags.ToString());

            var locationService = SpecUtil.CurrIocKernel.Get<LocationServiceInterface>();
            var box = locationService.GetBoundingBox(location, browserInfoService.Browser.Distance.GetValueOrDefault());

            Assert.IsNotNull(result, "no view result in context");
            var fliers = result;
            Assert.IsNotNull(fliers, "no fliers in context");

            Assert.IsTrue(fliers.Any());
            foreach (var bulletinFlierModel in fliers)
            {
                Assert.IsTrue(locationService.IsWithinBoundingBox(box, bulletinFlierModel.Location.ToDomainModel()));
                var tags = new Tags(bulletinFlierModel.TagsString);
                Assert.IsTrue(tags.Intersect(browserInfoService.Browser.Tags).Any());
            }
        }

        [Then(@"I should see FLIERS ordered by create date")]
        public void ThenIShouldSeeFliersOrderedByCreatetDate()
        {
            var flierList = SpecUtil.ControllerResult as IList<BulletinFlierModel>;
            Assert.IsNotNull(flierList);
            var flierListArray = flierList.ToArray();
            Assert.That(flierListArray.Count(), Is.EqualTo(3));

            for (int i = 1; i < flierListArray.Count(); i++)
            {
                Assert.IsTrue(
                    flierListArray[i - 1].CreateDate
                    <= flierListArray[i].CreateDate);
            }


        }

        #region Flier details

        [Then(@"I should see the public details of that FLIER")]
        public void ThenIShouldSeeThePublicDetailsOfThatFlier()
        {
            var viewRes = SpecUtil.ControllerResult as ViewResult;
            Assert.IsNotNull(viewRes);
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            Assert.IsNotNull(flier);
            var bulletinController = SpecUtil.GetController<BulletinApiController>();
            var retMod = bulletinController.Get(flier.FriendlyId);
            Assert.IsNotNull(retMod);

            var viewModelFact = SpecUtil.CurrIocKernel.Get<FlierBehaviourViewModelFactoryInterface>();
            var currMod = viewModelFact.GetBulletinViewModel(flier, true);
            AssertAreEqual(retMod.Flier, currMod);
            ScenarioContext.Current["fliermodel"] = retMod;
        }

        private void AssertAreEqual(BulletinFlierModel retMod, BulletinFlierModel currMod)
        {
            Assert.AreEqual(currMod.Id, retMod.Id);
            Assert.AreEqual(currMod.Title, retMod.Title);
            Assert.AreEqual(currMod.Description, retMod.Description);
            Assert.AreEqual(currMod.EffectiveDate, retMod.EffectiveDate);
            Assert.AreEqual(currMod.FlierBehaviour, retMod.FlierBehaviour);
        }

        [When(@"I have navigated to the public view page for that FLIER")]
        public void WhenIHaveNavigatedToTheViewPageForThatFlier()
        {
            var bulletinController = SpecUtil.GetController<BulletinController>();
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            SpecUtil.ControllerResult = bulletinController.Detail(flier.Id);
        }

        [Given("I have navigated to the public view page for a FLIER")]
        public void GivenIHaveNavigatedToThePublicViewPageForAFlier()
        {
            new FlierSteps().GivenIHaveCreatedAflier();
            WhenIHaveNavigatedToTheViewPageForThatFlier();
            ThenIShouldSeeThePublicDetailsOfThatFlier();
        }

        [Given("I have navigated to the public view page for a FLIER With TEAR OFF")]
        public void GivenIHaveNavigatedToThePublicViewPageForAFlierWithTearOff()
        {
            new FlierSteps().GivenAnotherBrowserHasCreatedAFLIERWithTEAROFF();
            WhenIHaveNavigatedToTheViewPageForThatFlier();
            ThenIShouldSeeThePublicDetailsOfThatFlier();
        }

        [Given(@"I have navigated to the public view page for a FLIER With TEAR OFF And USER CONTACT")]
        public void GivenIHaveNavigatedToThePublicViewPageForAFLIERWithTEAROFFAndUSERCONTACT()
        {
            new FlierSteps().GivenAnotherBrowserHasCreatedAFLIERWithTEAROFFAndUserContact();
            WhenIHaveNavigatedToTheViewPageForThatFlier();
            ThenIShouldSeeThePublicDetailsOfThatFlier();
        }

        //[Given("I have navigated to the public view page for a FLIER with Contact Details")]
        //public void GivenIHaveNavigatedToThePublicViewPageForAFlierWithContactDetails()
        //{
        //    new FlierSteps().GivenIHaveCreatedAflierWithContactDetails();
        //    WhenIHaveNavigatedToTheViewPageForThatFlier();
        //    ThenIShouldSeeThePublicDetailsOfThatFlier();
       // }


        #endregion




    }

    //still under dev
}
