using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using MbUnit.Framework;
using Ninject;
using TechTalk.SpecFlow;
using Website.Application.Extension.Validation;
using PostaFlya.Controllers;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Flier.Query;
using Website.Infrastructure.Command;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using PostaFlya.Specification.Browsers;
using PostaFlya.Specification.Util;
using PostaFlya.Mocks.Domain.Data;
using PostaFlya.Models.Content;
using Website.Infrastructure.Authentication;
using Website.Test.Common;
using System.Web;
using Website.Domain.Browser;
using Website.Domain.Location;
using Website.Domain.Tag;

namespace PostaFlya.Specification.Fliers
{
    [Binding]
    public class FlierSteps
    {
        private readonly CommonSteps _common = new CommonSteps();

        [Given(@"i have navigated to the CREATE PAGE for a FLIER TYPE (.*)")]
        public void GivenABrowserHasNavigatedToTheCreateFlierPage(string flierBehaviour)
        {
            var flierController = SpecUtil.GetController<FlierController>();
            var type = (FlierBehaviour)Enum.Parse(typeof(FlierBehaviour), flierBehaviour);
            var createModel = SpecUtil.CurrIocKernel.Get<FlierCreateModel>(ib => ib.Get<bool>("fliercreate"));
            createModel.FlierBehaviour = type;
            ScenarioContext.Current["createflya"] = createModel;
            flierController.Create(type);
        }

        [When(@"I SUBMIT the required data for a FLIER")]
        public void WhenISubmitTheRequiredDataForAFlier()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            WhenABrowserSubmitsTheRequiredDataForAFlier(browserInformation.Browser.Id);
        }

        public void WhenABrowserSubmitsTheRequiredDataForAFlier(string browserId)
        {
            var myFliersApiController = SpecUtil.GetApiController<MyFliersController>();
            var createModel = ScenarioContext.Current["createflya"] as FlierCreateModel;
            var test = myFliersApiController.Post(browserId, createModel);
            var flierid = test.EntityId();
            ScenarioContext.Current["createdflyaid"] = flierid;
        }


        [Then(@"the new FLIER will be created for behviour (.*)")]
        public void ThenTheNewFlierWillBeCreated(string flierBehaviour)
        {
            var flierid = ScenarioContext.Current["createdflyaid"] as string;
            Assert.IsNotNull(flierid);
            Assert.IsFalse(string.IsNullOrWhiteSpace(flierid));

            //var createModel = ScenarioContext.Current["createflya"] as FlierCreateModel;
            var flierQueryService = SpecUtil.CurrIocKernel.Get<FlierQueryServiceInterface>();
            var flier = flierQueryService.FindById<Flier>(flierid);

            var type = (FlierBehaviour)Enum.Parse(typeof(FlierBehaviour), flierBehaviour);
            Assert.AreEqual(type, flier.FlierBehaviour);

            Assert.IsNotNull(flier, "Flier Not Created");
            ScenarioContext.Current["flier"] = flier;
            Assert.Count(3, flier.ImageList);

        }

        [Then(@"the FLIER STATUS will be (.*)")]
        public void ThenTheFlierStatusWillBe(string status)
        {
            var flierStatus = (FlierStatus)Enum.Parse(typeof(FlierStatus), status, true);
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            Assert.IsNotNull(flier);
            Assert.AreEqual(flierStatus, flier.Status);
        }

        //REUSE
        [Given(@"There is a FLIER")]
        [Given(@"I have created a FLIER")]
        public void GivenIHaveCreatedAflier()
        {
            GivenIHaveCreatedAflierofBehaviour(FlierBehaviour.Default.ToString());
        }

        [Given(@"I have created a FLIER of BEHAVIOUR (.*)")]
        public void GivenIHaveCreatedAflierofBehaviour(string flierbehaviour)
        {           
            GivenABrowserHasNavigatedToTheCreateFlierPage(flierbehaviour);
            WhenISubmitTheRequiredDataForAFlier();
            ThenTheNewFlierWillBeCreated(flierbehaviour);
        }

        [Given(@"A BROWSER has created a FLIER of BEHAVIOUR (.*)")]
        public void GivenABrowserHasCreatedAFlierofBehaviour(string flierbehaviour)
        {
            var browserId = _common.GivenThereIsAnExistingBrowserWithParticipantRole();
            GivenABrowserHasNavigatedToTheCreateFlierPage(flierbehaviour);
            WhenABrowserSubmitsTheRequiredDataForAFlier(browserId);
            ThenTheNewFlierWillBeCreated(flierbehaviour);
        }

        [When(@"I navigate to the edit page for that FLIER and update any of the required data for a FLIER")]
        public void WhenINavigateToTheEditPageForThatFlierAndUpdate()
        {
            //var flierController = SpecUtil.GetController<FlierController>();
            //var flier = ScenarioContext.Current["flier"] as Flier;
            //var res = flierController.Edit(flier.Id) as ViewResult;

            //Assert.IsNotNull(res);
            //var flierEditModel = res.Model as FlierCreateModel;
            //Assert.IsNotNull(res);

            //flierEditModel.Id = flier.Id;
            //flierEditModel.Description = flier.Description + "UPDATED";
            //flierEditModel.Title = flier.Title + "UPDATED";
            //flierEditModel.TagsString = flier.Tags.ToString();
            //flierEditModel.Location = flier.Location.ToViewModel();

            var flierEditModel = FlierCreateModelFromFlier();
            flierEditModel.Description += "UPDATED";
            flierEditModel.Title += "UPDATED";


            var myFliersApiController = SpecUtil.GetController<MyFliersController>();
            var browserInformation = SpecUtil.GetCurrBrowser();
            myFliersApiController.Put(browserInformation.Browser.Id, flierEditModel);
        }

        [Then(@"the FLIER will be updated to reflect those changes")]
        public void ThenTheFlierWillBeUpdatedToReflectThoseChanges()
        {
            var flier = ScenarioContext.Current["flier"] as Flier;
            var flierQueryService = SpecUtil.CurrIocKernel.Get<FlierQueryServiceInterface>();
            var flierUpdatedId = flierQueryService
                .FindFliersByLocationTagsAndDistance(flier.Location, flier.Tags)
                .SingleOrDefault(id => flier.Id == id);

            var flierUpdated = flierQueryService.FindById<Flier>(flierUpdatedId);
            Assert.IsNotNull(flierUpdated, "Flier Not Updated");
            Assert.Contains(flierUpdated.Description, "UPDATED");
            Assert.Contains(flierUpdated.Title, "UPDATED");
        }


        [Given(@"I have navigated to the my fliers page")]
        [When(@"I navigate to the my fliers page")]
        public void HaveNavigatedToTheMyFliersPage()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();

            var browser = SpecUtil.CurrIocKernel.Get<BrowserInterface>(ctx => ctx.Has("defaultbrowser"));

            browserInformation.Browser = browser;

            var myFliersApiController = SpecUtil.GetController<MyFliersController>();
            SpecUtil.ControllerResult = myFliersApiController.Get(browserInformation.Browser.Id);
        }

        [When(@"I click on view for my FLIER")]
        public void WhenIClickOnViewForAFlier()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            var fliers = SpecUtil.ControllerResult as IQueryable<BulletinFlierModel>;

            var myFliersApiController = SpecUtil.GetController<MyFliersController>();
            SpecUtil.ControllerResult = myFliersApiController.Get(browserInformation.Browser.Id, fliers.First().Id);
        }


        [Then(@"I should see the details for my FLIER")]
        public void ThenIShouldSeeTheFLIERVIEWForThatFLIER()
        {
            var flier = SpecUtil.ControllerResult as FlierCreateModel ;
            Assert.IsNotNull(flier);
        }

        [Then(@"I should see a list of fliers I have created")]
        public void ThenIShouldSeeAListOfFliersIHaveCreated()
        {
            var fliers = SpecUtil.ControllerResult as IQueryable<BulletinFlierModel>;

            Assert.IsTrue(fliers.Count() == 3);
        }

        [When(@"i view the HEAT MAP")]
        public void WhenIViewTheHEATMAP()
        {
            var heatMapController = SpecUtil.GetApiController<HeatMapApiController>();
            var dataPoints = heatMapController.Get(new Location(145.0138751, -37.8799136).ToViewModel(),  20, new Tags(){"HEATMAP"}.ToString());
            ScenarioContext.Current["heatMapdata"] = dataPoints;
        }

        [Then(@"the Map should show points grouped by FLIER LOCATION")]
        public void ThenTheMapShouldShowPointsGroupedByFLIERLOCATION()
        {
            var dataPoints = ScenarioContext.Current["heatMapdata"] as IQueryable<HeatMapPoint>;
            if (dataPoints == null)
            {
                Assert.IsNull(dataPoints);
                return;
            }
            Assert.Count(1, dataPoints.Where(_ => Math.Abs(_.Longitude - 145.0138751) < 0.0001 && Math.Abs(_.Latitude - -37.8799136) < 0.0001).AsEnumerable());
            
        }

        [Then(@"have the weight SUMMED")]
        public void ThenHaveTheWeightSUMMED()
        {
            var dataPoints = ScenarioContext.Current["heatMapdata"] as IQueryable<HeatMapPoint>;
            if (dataPoints == null)
            {
                Assert.IsNull(dataPoints);
                return;
            }
            Assert.IsTrue(dataPoints.First(_ => Math.Abs(_.Longitude - 145.0138751) < 0.0001 && Math.Abs(_.Latitude - -37.8799136) < 0.0001).Weight == 100);
        }

        protected FlierCreateModel FlierCreateModelFromFlier()
        {
            var flier = ScenarioContext.Current["flier"] as Flier;
            var flierController = SpecUtil.GetController<FlierController>();
            var res = flierController.Edit(flier.Id) as ViewResult;

            Assert.IsNotNull(res);
            var flierEditModel = res.Model as FlierCreateModel;
            Assert.IsNotNull(res);

            flierEditModel.Id = flier.Id;
            flierEditModel.Description = flier.Description;
            flierEditModel.Title = flier.Title;
            flierEditModel.TagsString = flier.Tags.ToString();
            flierEditModel.Location = flier.Location.ToViewModel();

            return flierEditModel;
        }

        [When(@"I add images to the FLIER")]
        public void WhenIAddImagesToTheFLIER()
        {
            
            var flierEditModel = FlierCreateModelFromFlier();
            flierEditModel.ImageList = new List<ImageViewModel>() 
                                { 
                                    new ImageViewModel() { ImageId = Guid.NewGuid().ToString() }, 
                                    new ImageViewModel() { ImageId = Guid.NewGuid().ToString() }, 
                                    new ImageViewModel() { ImageId = Guid.NewGuid().ToString() } 
                                };
                            

            var myFliersApiController = SpecUtil.GetController<MyFliersController>();
            var browserInformation = SpecUtil.GetCurrBrowser();
            var test = myFliersApiController.Put(browserInformation.Browser.Id, flierEditModel);

            var flierid = test.EntityId();
            ScenarioContext.Current["createdflyaid"] = flierid;
        }

        [Then(@"The FLIER will contain the extra images")]
        public void ThenTheFLIERWillContainTheExtraImages()
        {
            var flierid = ScenarioContext.Current["createdflyaid"] as string;

            var flierQueryService = SpecUtil.CurrIocKernel.Get<FlierQueryServiceInterface>();
            var flier = flierQueryService.FindById<Flier>(flierid);
            Assert.Count(3, flier.ImageList);

        }

        [Given(@"I do not have a valid acces token to import fliers")]
        public void GivenIDoNotHaveAValidAccesTokenToImportFliers()
        {
            WhenIDontHaveAValidAccessTokenForTheGivenSource();

        }

        protected void AddCredentialsToCurrentBrowser(IdentityProviderCredential credential)
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            var browser = browserInformation.Browser as Browser;

            var browsCreds = new BrowserIdentityProviderCredential();
            browsCreds.CopyFieldsFrom(credential);
            browsCreds.BrowserId = browser.Id;

            browser.ExternalCredentials.Remove(browsCreds);
            browser.ExternalCredentials.Add(browsCreds);

            var accountController = SpecUtil.GetController<AccountController>();
            ControllerContextMock.FakeControllerContext(SpecUtil.CurrIocKernel, accountController);
            accountController.AddBrowser(browser);
        }

        [Then(@"Then I will be redirected to obtain a valid token")]
        public void ThenThenIWillBePromptedToObtainAValidToken()
        {
            var result = SpecUtil.ControllerResult as RedirectToRouteResult;
            Assert.AreEqual(result.RouteValues["controller"], "Account");
            Assert.AreEqual(result.RouteValues["action"], "RequestToken");
        }



        [Given(@"I dont have a valid access token for the given source")]
        public void WhenIDontHaveAValidAccessTokenForTheGivenSource()
        {
            // make current browser facebook access token expired

            var credentialsWithInvalidToken = new IdentityProviderCredential()
            {
                IdentityProvider = IdentityProviders.FACEBOOK,
                UserIdentifier = "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1hoFB",
                AccessToken = new AccessToken()
                {
                    Expires = DateTime.Now.AddDays(-1),
                    Permissions = "user_events",
                    Token = "abc123"
                }
            };

            AddCredentialsToCurrentBrowser(credentialsWithInvalidToken);

            var browserInformation = SpecUtil.GetCurrBrowser();
            var browser = browserInformation.Browser as Browser;

            var browserIdentityProviderCredential = browserInformation.Browser.ExternalCredentials.FirstOrDefault(_ => _.IdentityProvider == IdentityProviders.FACEBOOK);
            if (browserIdentityProviderCredential != null)
            {
                var tokenExpires = browserIdentityProviderCredential.AccessToken.Expires;

                Assert.IsTrue(DateTime.Now > tokenExpires);
            }
        }

        [When(@"I go to the flier import page for a source")]
        public void WhenIGoToTheFlierImportPageForASource()
        {
            var fliersImportController = SpecUtil.GetController<FlierImportController>();
            SpecUtil.ControllerResult = fliersImportController.Import(IdentityProviders.FACEBOOK);
            
        }

        [Then(@"Then potential fliers that have not already been imported with be displayed")]
        public void ThenThenPotentialFliersThatHaveNotAlreadyBeenImportedWithBeDisplayed()
        {
            var result = SpecUtil.ControllerResult as ViewResult;
            var importedFliers = result.Model as IQueryable<FlierCreateModel>;
            Assert.Count(5, importedFliers);
            //Assert.Count(5, importedFliers.Where(_ => _. == FlierStatus.Pending));
        }

        [Given(@"I have a valid access token for the given source")]
        public void WhenIHaveAValidAccessTokenForTheGivenSource()
        {
         
            // make current browser facebook access token expired

            var credentialsWithInvalidToken = new IdentityProviderCredential()
            {
                IdentityProvider = IdentityProviders.FACEBOOK,
                UserIdentifier = "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1ho",
                AccessToken = new AccessToken()
                {
                    Expires = DateTime.Now.AddDays(1),
                    Permissions = "user_events",
                    Token = "abc123"
                }
            };

            AddCredentialsToCurrentBrowser(credentialsWithInvalidToken);

            var browserInformation = SpecUtil.GetCurrBrowser();
            var browser = browserInformation.Browser as Browser;

            var identityProviderCredential = browserInformation.Browser.ExternalCredentials.FirstOrDefault(_ => _.IdentityProvider == IdentityProviders.FACEBOOK);
            if (identityProviderCredential != null)
            {
                var tokenExpires = identityProviderCredential.AccessToken.Expires;

                Assert.IsTrue(DateTime.Now < tokenExpires);
            }
        }



    }
}
