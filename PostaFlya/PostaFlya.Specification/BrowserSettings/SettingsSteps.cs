using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using MbUnit.Framework;
using Ninject;
using TechTalk.SpecFlow;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Controllers;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Content.Command;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.Tag;
using PostaFlya.Models;
using PostaFlya.Models.Location;
using PostaFlya.Models.Tags;
using PostaFlya.Specification.Browsers;
using PostaFlya.Specification.DynamicBulletinBoard;
using PostaFlya.Specification.Util;

namespace PostaFlya.Specification.BrowserSettings
{
    [Binding]
    public class SettingsSteps
    {
        private CommonSteps _common = new CommonSteps();

        #region SAVED LOCATIONS

        [When(@"i Delete a Location from my SAVED Locations")]
        public void WhenIDeleteALocationFromMySavedLocations()
        {
            WhenIAddALocationToMySavedLocations();
            var locationsApiController = SpecUtil.GetApiController<SavedLocationsController>();
            var browserInformation = SpecUtil.GetCurrBrowser();
            locationsApiController.Delete(browserInformation.Browser.Id, new Location(-23, 23));
        }

        [Then(@"it should be removed from my SAVED LOCATIONS")]
        public void ThenItShouldBeRemovedFromMySavedLocations()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            Assert.IsTrue(!browserInformation.Browser.SavedLocations.Contains(new Location(-23, 23)));
            
        }

        [When(@"i ADD a Location to my SAVED Locations")]
        public void WhenIAddALocationToMySavedLocations()
        {
            var locationsApiController = SpecUtil.GetApiController<SavedLocationsController>();
            var browserInformation = SpecUtil.GetCurrBrowser();
            locationsApiController.Post(browserInformation.Browser.Id, new LocationModel(new Location(-23, 23)));

        }

        [Then(@"The LOCATION should be stored against my BROWSER")]
        public void ThenTheLocationShouldBeStoredAgainstMyBrowser()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            Assert.IsTrue(browserInformation.Browser.SavedLocations.Count(l => Math.Abs(l.Latitude - 23) < 0.01 && Math.Abs(l.Longitude - -23) < 0.01) > 0 );

        }

        [When(@"i navigate to the BROWSER SAVED LOCATIONS page")]
        public void WhenINavigateToTheBrowserSavedLocationsPage()
        {
            var locationsApiController = SpecUtil.GetController<SavedLocationsController>();
            var browserInformation = SpecUtil.GetCurrBrowser();
            SpecUtil.ControllerResult = locationsApiController.Get(browserInformation.Browser.Id);
        }

        [Then(@"i should have a list of my SAVED LOCATIONS")]
        public void ThenIShouldHaveAListOfMySavedLocations()
        {
            var savedLocationList = SpecUtil.ControllerResult;
            Assert.IsNotNull(savedLocationList);
            Assert.IsInstanceOfType(typeof(IQueryable<LocationModel>), savedLocationList);
        }

         [When(@"i Select a SAVED LOCATION")]
        public void WhenISelectASavedLocation()
        {
            WhenIAddALocationToMySavedLocations();
            var locationsApiController = SpecUtil.GetController<SavedLocationsController>();
            var browserInformation = SpecUtil.GetCurrBrowser();
            locationsApiController.Put(browserInformation.Browser.Id, new LocationModel(new Location(-23, 23)));
        }

        [Then(@"it should become my currently selected LOCATION")]
        public void ThenItShouldBecomeMyCurrentlySelectedLocation()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            Assert.IsTrue(browserInformation.Browser.DefaultLocation.Equals(new Location(-23, 23)));
        }

        [Given(@"I have navigated to the settings page to enter a DEFAULT LOCATION")]
        public void GivenIHaveNavigatedToTheSettingsPageToEnterADEFAULTLOCATION()
        {
            var browserSettingController = SpecUtil.GetController<BrowserController>();
            var viewResult = browserSettingController.SetLocation() as ViewResult;
        }

        [When(@"I enter a DEFAULT LOCATION")]
        public void WhenIEnterADEFAULTLOCATION()
        {
            var browserSettingController = SpecUtil.GetController<BrowserController>();
            var location = new Location(-30, -30);
            browserSettingController.SetLocation(location);
        }

        [Then(@"the DEFAULT LOCATION will be stored against the BROWSER")]
        public void ThenItWillBeStoredAgainstTheBROWSER()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            Assert.IsTrue(Math.Abs(browserInformation.Browser.DefaultLocation.Longitude - -30) < 0.01 && Math.Abs(browserInformation.Browser.DefaultLocation.Latitude - -30) < 0.01);
        }

        #endregion SAVED LOCATIONS

        #region SAVED TAG SETS

        [When(@"I save the TAG SET to my Saved TAG SETs")]
        public void WhenIaddATagSetToMySavedTagSets()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            var savedTagsApiController = SpecUtil.GetApiController<SavedTagsController>();
            savedTagsApiController.Post(browserInformation.Browser.Id, browserInformation.Browser.Tags);

        }

        [Then(@"The TAG SET should be stored against my BROWSER")]
        public void ThenTheTagSetShouldBeStoredAgainstMyBrowser()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            Assert.Contains(browserInformation.Browser.SavedTags, browserInformation.Browser.Tags);
        }

        [When(@"i Select a SAVED TAG SET")]
        public void WhenISelectASavedTagSet()
        {
            //switch to a different set of tags
            new DynamicBulletinBoardSteps().SetSomeTagsSet("defaulttags2");
            var savedTagsApiController = SpecUtil.GetController<SavedTagsController>();
            var defaultTags = SpecUtil.CurrIocKernel.Get<Tags>(ib => ib.Get<bool>("default"));
            var browserInformation = SpecUtil.GetCurrBrowser();
            savedTagsApiController.Put(browserInformation.Browser.Id, defaultTags);
        }

        [Then(@"it should become my currently selected TAG SET")]
        public void ThenItShouldBecomeMyCurrentlySelectedTagSet()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            var defaultTags = SpecUtil.CurrIocKernel.Get<Tags>(ib => ib.Get<bool>("default"));
            Assert.AreEqual(defaultTags, browserInformation.Browser.Tags);
        }

        [Given(@"I have at least one TAG SET in my SAVED TAG SETS")]
        public void GivenIHaveAtLeastOneTagSetSaved()
        {
            _common.GivenIamABrowserInParticipantRole();
            new DynamicBulletinBoardSteps().GivenThereAreSomeTagsSet();
            WhenIaddATagSetToMySavedTagSets();
            ThenTheTagSetShouldBeStoredAgainstMyBrowser();
        }

        [When(@"i Delete a TAG SET from my SAVED TAG SETS")]
        public void WhenIDeleteAtagsetFromMySavedTagSets()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            var savedTagsApiController = SpecUtil.GetController<SavedTagsController>();

            savedTagsApiController.Delete(browserInformation.Browser.Id, browserInformation.Browser.SavedTags[0]);
        }

        [Then(@"it should be removed from my SAVED TAG SETS")]
        public void ThenItShouldBeRemovedFromMySavedTagSets()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            Assert.Count(0, browserInformation.Browser.SavedTags);
        }

        [When(@"i navigate to the BROWSER SAVED TAG SETS page")]
        public void WhenINavigateToTheBrowserSavedTagSetsPage()
        {
            var savedTagsApiController = SpecUtil.GetController<SavedTagsController>();
            var browserInformation = SpecUtil.GetCurrBrowser();
            SpecUtil.ControllerResult = savedTagsApiController.Get(browserInformation.Browser.Id);
        }

        [Then(@"i should have a list of my SAVED TAG SETS")]
        public void ThenIShouldHaveAListOfMySavedtagSets()
        {
            var tagList = SpecUtil.ControllerResult as IQueryable<Tags>;
            Assert.IsTrue(tagList.Any());
        }

        #endregion SAVED TAG SETS

        #region My IMAGES STEPS

        [Then(@"i should have a list of my SAVED IMAGES")]
        public void ThenIShouldHaveAListOfMySAVEDIMAGES()
        {
            var browserInformation = SpecUtil.CurrIocKernel.Get<BrowserInformationInterface>();
            var savedImagesApiController = SpecUtil.GetController<MyImagesController>();

            var imageRepo = SpecUtil.CurrIocKernel.Get<ImageRepositoryInterface>();
            var testImage = new Image()
                             {
                                 Id = Guid.NewGuid().ToString(),
                                 Title = "test",
                                 BrowserId = browserInformation.Browser.Id,
                                 Status = ImageStatus.Processing,
                                 Location = new Location(0,0)
                             };
            imageRepo.Store(testImage);

            var imageList = savedImagesApiController.Get(browserInformation.Browser.Id);



            Assert.IsTrue(imageList.Count == 1);
        }


        #endregion

    }
}
