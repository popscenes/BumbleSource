using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Ninject;
using PostaFlya.Domain.Browser;
using TechTalk.SpecFlow;
using PostaFlya.Controllers;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using PostaFlya.Specification.Fliers;
using PostaFlya.Specification.Util;
using Website.Common.Model.Query;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Query;
using Website.Test.Common;

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
            if(browserInfoService.Browser == null)
            {
                var defBrows = SpecUtil.CurrIocKernel.Get<BrowserInterface>(ctx => ctx.Has("postadefaultbrowser"));
                ScenarioContext.Current["browserId"] = defBrows.Id;
            }

            var dateFilter = ScenarioContext.Current.ContainsKey("eventfilterdate") ? ScenarioContext.Current["eventfilterdate"] as DateTime? : null;
            var distance = ScenarioContext.Current.ContainsKey("currentdistance")
                   ? (int)ScenarioContext.Current["currentdistance"]
                   : 0;
            var tags = ScenarioContext.Current.ContainsKey("currenttags")
                               ? (string)ScenarioContext.Current["currenttags"]
                               : null;


            SpecUtil.ControllerResult = bulletinController
                .Get(new BulletinGetRequestModel()
                    {
                        Loc = location.ToViewModel(),
                        Count = 30,
                        Board = "",
                        Distance = distance,
                        Tags = tags,
                        Date = dateFilter
                    });
        }

        [Given(@"I have navigated to the public view page for a FLIER from the BULLETIN BOARD")]
        [When(@"I navigate to the public view page for a FLIER from the BULLETIN BOARD")]
        public void WhenIHaveNavigatedToThePublicViewPageForAFLIERFromTheBULLETINBOARD()
        {
            var fliers = SpecUtil.ControllerResult as IList<BulletinFlierSummaryModel>;
            var flier = fliers.FirstOrDefault();
            Assert.IsNotNull(flier, "no fliers in context");

            var bulletinApiController = SpecUtil.GetController<BulletinApiController>();
            var mod = bulletinApiController.Get(flier.FriendlyId);
            ScenarioContext.Current["fliermodel"] = mod;
        }


        //REUSE
        [Given(@"there are some TAGS set")]
        public void GivenThereAreSomeTagsSet()
        {
            SetSomeTagsSet();
        }

        public void SetSomeTagsSet(string testSetName = "default")
        {
            ScenarioContext.Current["currenttags"] = SpecUtil.CurrIocKernel.Get<Tags>(ib => ib.Get<bool>(testSetName)).ToString();
        }

        [Then(@"I should only see FLIERS within a DISTANCE from that LOCATION")]
        public void ThenIShouldSeeFliersWithinADistanceFromThatLocation()
        {
            var fliers = SpecUtil.ControllerResult as IList<BulletinFlierSummaryModel>;
            Assert.IsNotNull(fliers, "no fliers in context");

            var locService = SpecUtil.CurrIocKernel.Get<LocationServiceInterface>();
            foreach (var flier in fliers)
            {
                Assert.IsTrue(locService.IsWithinDefaultDistance(flier.Venue.Address.ToDomainModel()),
                              "Flier returned that isn't within default distance");
            }

            Assert.That(fliers.Count(), Is.GreaterThanOrEqualTo(1));
        }

        [Then(@"I should only see FLIERS with matching TAGS")]
        public void ThenIShouldSeeFliersWithMatchingTags()
        {
            var fliers = SpecUtil.ControllerResult as IList<BulletinFlierSummaryModel>;
            Assert.IsNotNull(fliers, "no fliers in context");

            var defaultTags = SpecUtil.CurrIocKernel.Get<Tags>(ib => ib.Get<bool>("default"));
            foreach (var flier in fliers)
            {
                Assert.IsTrue(defaultTags.Union(new Tags(flier.TagsString)).Any(),
                              "Flier returned that doesn't match tags %s", flier);
            }

            Assert.That(fliers.Count(), Is.EqualTo(30));
        }

        [When(@"I set my DISTANCE")]
        public void WhenISetMyDISTANCE()
        {
            ScenarioContext.Current["currentdistance"] = 15;
        }

        [Then(@"i should see all fliers within my new DISTANCE that have matching TAGS")]
        public void ThenIShouldSeeAllFliersWithinMyNewDISTANCEThatHaveMatchingTAGS()
        {
            var bulletinApiController = SpecUtil.GetController<BulletinApiController>();
            var location = SpecUtil.CurrIocKernel.Get<Location>(ib => ib.Get<bool>("default"));
            var browserInfoService = SpecUtil.GetCurrBrowser();

            var distance = ScenarioContext.Current.ContainsKey("currentdistance")
                               ? (int) ScenarioContext.Current["currentdistance"]
                               : 0;
            var tags = ScenarioContext.Current.ContainsKey("currenttags")
                               ? (string)ScenarioContext.Current["currenttags"]
                               : null;
            var result = bulletinApiController.Get(
                new BulletinGetRequestModel()
                    {
                        Loc = location.ToViewModel(),
                        Count = 30,
                        Board = "",
                        Distance = distance,
                        Tags = tags
                    });
                
            var locationService = SpecUtil.CurrIocKernel.Get<LocationServiceInterface>();
            var box = locationService.GetBoundingBox(location, distance);

            Assert.IsNotNull(result, "no view result in context");
            var fliers = result;
            Assert.IsNotNull(fliers, "no fliers in context");

            Assert.IsTrue(fliers.Any());
            foreach (var bulletinFlierModel in fliers)
            {
                Assert.IsTrue(locationService.IsWithinBoundingBox(box, bulletinFlierModel.Venue.Address.ToDomainModel()));
                var tagsret = new Tags(bulletinFlierModel.TagsString);
                Assert.IsTrue(tagsret.Union(new Tags(tags)).Any());
            }
        }

        [Then(@"I should see FLIERS ordered by create date")]
        public void ThenIShouldSeeFliersOrderedByCreatetDate()
        {
            var flierList = SpecUtil.ControllerResult as IList<BulletinFlierSummaryModel>;
            Assert.IsNotNull(flierList);
            var flierListArray = flierList.ToArray();
            Assert.That(flierListArray.Count(), Is.EqualTo(30));

            for (int i = 1; i < flierListArray.Count(); i++)
            {
                Assert.IsTrue(
                    flierListArray[i - 1].CreateDate
                    <= flierListArray[i].CreateDate);
            }


        }

        [Given(@"I set the event date filter")]
        public void WhenISetTheEventDateFilter()
        {
            ScenarioContext.Current["eventfilterdate"] = new DateTime(2076, 8, 11);
        }

        [Then(@"I should see only FLIERS with that event date")]
        public void ThenIShouldSeeOnlyFLIERSWithThatEventDate()
        {
            var flierList = SpecUtil.ControllerResult as IList<BulletinFlierSummaryModel>;
            Assert.IsNotNull(flierList);
            var flierListArray = flierList.ToArray();

            Assert.That(flierListArray.Count(), Is.GreaterThanOrEqualTo(1));
            AssertUtil.AreAll(flierListArray, model => 
                model.EventDates.Any(d => d == new DateTime(2076, 8, 11)));
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

            var viewModelFact = SpecUtil.CurrIocKernel.Get<QueryChannelInterface>();
            var currMod = viewModelFact.ToViewModel<BulletinFlierDetailModel>(flier);
            AssertAreEqual(retMod.Flier, currMod);
            ScenarioContext.Current["fliermodel"] = retMod;
        }

        private void AssertAreEqual(BulletinFlierDetailModel retMod, BulletinFlierDetailModel currMod)
        {
            Assert.AreEqual(currMod.Id, retMod.Id);
            Assert.AreEqual(currMod.Title, retMod.Title);
            Assert.AreEqual(currMod.Description, retMod.Description);
            Assert.AreEqual(currMod.EventDates, retMod.EventDates);
        }

        [When(@"I have navigated to the public view page for that FLIER")]
        public void WhenIHaveNavigatedToTheViewPageForThatFlier()
        {
            var bulletinController = SpecUtil.GetController<BulletinController>();
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            SpecUtil.ControllerResult = bulletinController.Detail(flier.FriendlyId);
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
            new FlierSteps().GivenABrowserHasCreatedAFlier();
            WhenIHaveNavigatedToTheViewPageForThatFlier();
            ThenIShouldSeeThePublicDetailsOfThatFlier();
        }

//        [Given(@"I have navigated to the public view page for a FLIER With TEAR OFF And USER CONTACT")]
//        public void GivenIHaveNavigatedToThePublicViewPageForAFLIERWithTEAROFFAndUSERCONTACT()
//        {
//            new FlierSteps().GivenAnotherBrowserHasCreatedAFLIERWithTEAROFFAndUserContact();
//            WhenIHaveNavigatedToTheViewPageForThatFlier();
//            ThenIShouldSeeThePublicDetailsOfThatFlier();
//        }

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
