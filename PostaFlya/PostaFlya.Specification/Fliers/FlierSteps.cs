﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Ninject;
using PostaFlya.Domain.Flier.Query;
using TechTalk.SpecFlow;
using PostaFlya.Controllers;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;
using PostaFlya.Models.Flier;
using PostaFlya.Models.Location;
using PostaFlya.Specification.Util;
using PostaFlya.Models.Content;
using Website.Application.Domain.Content;
using Website.Domain.Browser.Query;
using Website.Domain.Payment;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
using Website.Test.Common;
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
        public void GivenIOrAnotherBrowserHasNavigatedToTheCreateFlierPage(string flierBehaviour)
        {
            var flierController = SpecUtil.GetController<FlierController>();
            var type = (FlierBehaviour)Enum.Parse(typeof(FlierBehaviour), flierBehaviour);
            var createModel = SpecUtil.CurrIocKernel.Get<FlierCreateModel>(ib => ib.Get<bool>("fliercreate"));
            createModel.FlierBehaviour = type;
            ScenarioContext.Current["createflya"] = createModel;
            flierController.Create(type);
        }

       // [Given(@"I choose to attach my default contact details")]
        //public void AndIChooseToAttachMyDefaultContactDetails()
        //{
         //   var createModel = ScenarioContext.Current["createflya"] as FlierCreateModel;
         //   createModel.AttachContactDetails = true;
        //}

        [Then(@"contact details will be retrievable for the FLIER")]
        public void ThenContactDetailsWillBeRetrievableForTheFlier()
        {
            var createdFlier = ScenarioContext.Current["flier"] as FlierInterface;
            Assert.IsNotNull(createdFlier);//test ThenTheNewFlierWillBeCreated(string flierBehaviour) first

            Assert.IsTrue(createdFlier.HasContactDetails());
            
        }

        [When(@"I SUBMIT the data for that FLIER")]
        public void WhenISubmitTheRequiredDataForAFlier()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            WhenTheBrowserSubmitsTheRequiredDataForAFlier(browserInformation.Browser.Id);
        }

        public void WhenTheBrowserSubmitsTheRequiredDataForAFlier(string browserId)
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
            var flierQueryService = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();
            var flier = flierQueryService.FindById<Flier>(flierid);

            var type = (FlierBehaviour)Enum.Parse(typeof(FlierBehaviour), flierBehaviour);
            Assert.AreEqual(type, flier.FlierBehaviour);

            Assert.IsNotNull(flier, "Flier Not Created");
            ScenarioContext.Current["flier"] = flier;
            Assert.That(flier.ImageList.Count(), Is.EqualTo(3));

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

        //[Given(@"There is a FLIER with Contact Details")]
        //[Given(@"I have created a FLIER with Contact Details")]
        //public void GivenIHaveCreatedAflierWithContactDetails()
        //{
         //   GivenIHaveCreatedAflierofBehaviourWithContactDetails(FlierBehaviour.Default.ToString());
        //}

        //[Given(@"I have created a FLIER of BEHAVIOUR (.*) with Contact Details")]
        //public void GivenIHaveCreatedAflierofBehaviourWithContactDetails(string flierbehaviour)
        //{
         //   GivenIOrAnotherBrowserHasNavigatedToTheCreateFlierPage(flierbehaviour);
         //   AndIChooseToAttachMyDefaultContactDetails();
         //   WhenISubmitTheRequiredDataForAFlier();
         //   ThenTheNewFlierWillBeCreated(flierbehaviour);
       // }

        [Given(@"I have created a FLIER of BEHAVIOUR (.*)")]
        public void GivenIHaveCreatedAflierofBehaviour(string flierbehaviour)
        {           
            GivenIOrAnotherBrowserHasNavigatedToTheCreateFlierPage(flierbehaviour);
            WhenISubmitTheRequiredDataForAFlier();
            ThenTheNewFlierWillBeCreated(flierbehaviour);
        }

        [Given(@"A BROWSER has created a FLIER")]
        public void GivenABrowserHasCreatedAFlierofBehaviour()
        {
            var browserId = _common.GivenThereIsAnExistingBrowserWithParticipantRole();
            GivenIOrAnotherBrowserHasNavigatedToTheCreateFlierPage(FlierBehaviour.Default.ToString());
            WhenTheBrowserSubmitsTheRequiredDataForAFlier(browserId);
            ThenTheNewFlierWillBeCreated(FlierBehaviour.Default.ToString());
        }

        [Given(@"A BROWSER has created a FLIER of BEHAVIOUR (.*)")]
        public void GivenABrowserHasCreatedAFlierofBehaviour(string flierbehaviour)
        {
            var browserId = _common.GivenThereIsAnExistingBrowserWithParticipantRole();
            GivenIOrAnotherBrowserHasNavigatedToTheCreateFlierPage(flierbehaviour);
            WhenTheBrowserSubmitsTheRequiredDataForAFlier(browserId);
            ThenTheNewFlierWillBeCreated(flierbehaviour);
        }

        [When(@"I navigate to the edit page for that FLIER and update any of the required data for a FLIER")]
        public void WhenINavigateToTheEditPageForThatFlierAndUpdate()
        {
            var flierEditModel = FlierCreateModelFromFlier();
            flierEditModel.Description += "UPDATED";
            flierEditModel.Title += "UPDATED";

            ScenarioContext.Current["flierEditModel"] = flierEditModel;

            var myFliersApiController = SpecUtil.GetController<MyFliersController>();
            var browserInformation = SpecUtil.GetCurrBrowser();
            myFliersApiController.Put(browserInformation.Browser.Id, flierEditModel);
        }

        [Then(@"the FLIER will be updated to reflect those changes")]
        public void ThenTheFlierWillBeUpdatedToReflectThoseChanges()
        {
            var flier = ScenarioContext.Current["flier"] as Flier;
            var flierQueryService = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();
            var flierSearchService = SpecUtil.CurrIocKernel.Get<FlierSearchServiceInterface>();

            var flierUpdatedId = flierSearchService
                .FindFliersByLocationTagsAndDistance(flier.Location, flier.Tags)
                .SingleOrDefault(id => flier.Id == id);

            var flierUpdated = flierQueryService.FindById<Flier>(flierUpdatedId);
            Assert.IsNotNull(flierUpdated, "Flier Not Updated");
            var editMod = ScenarioContext.Current["flierEditModel"] as FlierCreateModel;

            Assert.AreEqual(editMod.Description, flierUpdated.Description);
            Assert.AreEqual(editMod.Title, flierUpdated.Title);
            Assert.AreEqual(editMod.AttachTearOffs, flierUpdated.HasContactDetails());
            Assert.AreEqual(editMod.AllowUserContact, flierUpdated.HasLeadGeneration);

            ScenarioContext.Current["flier"] = flierUpdated;
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
            var fliers = SpecUtil.ControllerResult as IQueryable<FlierCreateModel>;

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
            var fliers = SpecUtil.ControllerResult as IQueryable<FlierCreateModel>;

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

            var count =
                dataPoints.Count(_ => Math.Abs(_.Longitude - 145.0138751) < 0.0001 && Math.Abs(_.Latitude - -37.8799136) < 0.0001);
            Assert.That(count, Is.EqualTo(1));            
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
            flierEditModel.FlierImageId = flier.Image.Value.ToString();
            flierEditModel.AttachTearOffs = flier.HasContactDetails();
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

            var flierQueryService = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();
            var flier = flierQueryService.FindById<Flier>(flierid);
            Assert.That(flier.ImageList.Count(), Is.EqualTo(3));
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
            Assert.That(importedFliers.Count(), Is.EqualTo(5));
            //AssertUtil.Count(5, importedFliers.Where(_ => _. == FlierStatus.Pending));
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

        [Given(@"I choose to attach Tear Offs")]
        public void GivenIOrAnotherBrowserChooseToAttachTearOffs()
        {
            var createFlierModel = ScenarioContext.Current["createflya"] as FlierCreateModel;
            createFlierModel.AttachTearOffs = true;
        }

        [Then(@"the FLIER will contain a FEATURE described as (.*) with a cost of (.*) credits")]
        public void ThenTheFLIERWillContainAFEATUREForTearOffInAEnabledState(string featureDescription, string cost)
        {
            featureDescription = featureDescription.Trim('\"');
            var creditCost = Int32.Parse(cost);
            
            var flierid = ScenarioContext.Current["createdflyaid"] as string;

            var flierQueryService = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();
            var flier = flierQueryService.FindById<Flier>(flierid);

            Assert.AreEqual(flier.Features.Count, 2);
            var flierFeature = flier.Features.FirstOrDefault(f => f.Description.Equals(featureDescription));
            Assert.NotNull(flierFeature, "no feature found with description " + featureDescription);
            Assert.AreEqual(flierFeature.Cost, creditCost);
        }

        [Given(@"I choose to allow User Contact")]
        public void GivenIOrAnotherBrowserChooseToAllowUserContact()
        {
            var createFlierModel = ScenarioContext.Current["createflya"] as FlierCreateModel;
            createFlierModel.AllowUserContact = true;
        }

        [When(@"I navigate to the edit page for that FLIER and add TEAR OFF to a FLIER")]
        public void WhenINavigateToTheEditPageForThatFLIERAndAddTEAROFFToAFLIER()
        {
            EditFlierWithFeatures(true, false);
        }

        [Given(@"I have created a FLIER with TEAR OFF")]
        public void GivenIHaveCreatedAFLIERWithTEAROFF()
        {
            GivenIOrAnotherBrowserHasNavigatedToTheCreateFlierPage("Default");
            GivenIOrAnotherBrowserChooseToAttachTearOffs();
            _common.GivenIHaveAccountCredit(1000);
            WhenISubmitTheRequiredDataForAFlier();
            ThenTheNewFlierWillBeCreated("Default");
        }

        public void GivenAnotherBrowserHasCreatedAFLIERWithTEAROFF()
        {
            var browserId = _common.GivenThereIsAnExistingBrowserWithParticipantRole();
            GivenIOrAnotherBrowserHasNavigatedToTheCreateFlierPage("Default");
            GivenIOrAnotherBrowserChooseToAttachTearOffs();
            _common.GivenABrowserHasAccountCredit(browserId, 1000);
            WhenTheBrowserSubmitsTheRequiredDataForAFlier(browserId);
            ThenTheNewFlierWillBeCreated("Default");
        }

        public void GivenIHaveCreatedAFLIERWithTEAROFFAndUserContact()
        {
            GivenIOrAnotherBrowserHasNavigatedToTheCreateFlierPage("Default");
            GivenIOrAnotherBrowserChooseToAttachTearOffs();
            GivenIOrAnotherBrowserChooseToAllowUserContact();
            _common.GivenIHaveAccountCredit(1000);
            WhenISubmitTheRequiredDataForAFlier();
            ThenTheNewFlierWillBeCreated("Default");
        }

        public void GivenAnotherBrowserHasCreatedAFLIERWithTEAROFFAndUserContact()
        {
            var browserId = _common.GivenThereIsAnExistingBrowserWithParticipantRole();
            GivenIOrAnotherBrowserHasNavigatedToTheCreateFlierPage("Default");
            GivenIOrAnotherBrowserChooseToAttachTearOffs();
            GivenIOrAnotherBrowserChooseToAllowUserContact();
            _common.GivenABrowserHasAccountCredit(browserId, 1000);
            WhenTheBrowserSubmitsTheRequiredDataForAFlier(browserId);
            ThenTheNewFlierWillBeCreated("Default");
        }

        [Given(@"I have created a FLIER with USER CONTACT")]
        public void GivenIHaveCreatedAFLIERWithUSERCONTACT()
        {
            GivenIOrAnotherBrowserHasNavigatedToTheCreateFlierPage("Default");
            GivenIOrAnotherBrowserChooseToAllowUserContact();
            _common.GivenIHaveAccountCredit(1000);
            WhenISubmitTheRequiredDataForAFlier();
            ThenTheNewFlierWillBeCreated("Default");
        }

        private void EditFlierWithFeatures(bool AttachTearOffs, bool AllowUserContact)
        {
            var flierEditModel = FlierCreateModelFromFlier();

            flierEditModel.AttachTearOffs = AttachTearOffs;
            flierEditModel.AllowUserContact = AllowUserContact;
            flierEditModel.Description += "UPDATED";
            flierEditModel.Title += "UPDATED";

            var myFliersApiController = SpecUtil.GetController<MyFliersController>();
            var browserInformation = SpecUtil.GetCurrBrowser();
            myFliersApiController.Put(browserInformation.Browser.Id, flierEditModel);
            ScenarioContext.Current["flierEditModel"] = flierEditModel;
        }

        [When(@"I navigate to the edit page for that FLIER and remove TEAR OFF to a FLIER")]
        public void WhenINavigateToTheEditPageForThatFLIERAndRemoveTEAROFFToAFLIER()
        {
            EditFlierWithFeatures(false, false);
        }

        [Then(@"the FLIER will not contain a FEATURE described as (.*)")]
        public void ThenTheFLIERWillNotContainAFEATUREForTearOff(string featureDesc)
        {
 

            var flierid = ScenarioContext.Current["createdflyaid"] as string;

            var flierQueryService = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();
            var flier = flierQueryService.FindById<Flier>(flierid);

            var feature = flier.Features.FirstOrDefault(_ => _.Description == featureDesc);

            Assert.IsNull(feature);
        }

        [When(@"I navigate to the edit page for that FLIER and add USER CONTACT to a FLIER")]
        public void WhenINavigateToTheEditPageForThatFLIERAndAddUSERCONTACTToAFLIER()
        {
            EditFlierWithFeatures(false, true);
        }


        [When(@"I navigate to the edit page for that FLIER and remove USER CONTACT to a FLIER")]
        public void WhenINavigateToTheEditPageForThatFLIERAndRemoveUSERCONTACTToAFLIER()
        {
            EditFlierWithFeatures(false, false);
        }


        [When(@"I choose to download the printable flier image")]
        public void WhenIChooseToDownloadThePrintableFlierImage()
        {
            var imgController = SpecUtil.GetController<ImgController>();
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            SpecUtil.ControllerResult = imgController.GetPrintFlier(flier.Id);
        }

        [Then(@"I should recieve the printable flier image")]
        public void ThenIShouldRecieveThePrintableFlierImage()
        {
            var ret = SpecUtil.ControllerResult as FileContentResult;
            Assert.IsNotNull(ret);
            using (var ms = new MemoryStream(ret.FileContents))
            {
                using (var img = Image.FromStream(ms))
                {                  
                    Assert.That(img.Size, Is.EqualTo(ImageUtil.A4300DpiSize));
                }
            }            
        }

        [Then(@"It should have a unique Tiny Url")]
        public void ThenItShouldHaveAUniqueTinyUrl()
        {
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            Assert.IsNotNull(flier);
            Assert.IsNotNullOrEmpty(flier.TinyUrl);
        }

        [Then(@"A CREDIT TRANSACTION of type createFlier will be created")]
        public void ThenACREDITTRANSACTIONOfTypeCreateFlierWillBeCreated()
        {
            var creditTransactionsQueryService = SpecUtil.CurrIocKernel.Get<GenericQueryServiceInterface>();
            var browserInformation = SpecUtil.GetCurrBrowser();
            var transactions = creditTransactionsQueryService.FindAggregateEntities<CreditTransaction>(browserInformation.Browser.Id);
            Assert.IsTrue(transactions.First().CreditTransactionType == "Posting a flier to an area");
            Assert.IsTrue(transactions.First().Credits == 80);
        }

        [Given(@"I Create Flier With With Insufficient Credit")]
        public void GivenICreateFlierWithWithInsufficientCredit()
        {
            GivenIOrAnotherBrowserHasNavigatedToTheCreateFlierPage(FlierBehaviour.Default.ToString());
            _common.GivenIHaveAccountCredit(0);
            WhenISubmitTheRequiredDataForAFlier();
        }

        [When(@"I navigate to the Pendng Fliers Page")]
        public void WhenINavigateToThePendngFliersPage()
        {

            var profileController = SpecUtil.GetController<ProfileController>();
            SpecUtil.ControllerResult = profileController.PaymentPending();
        }

        [Then(@"I willo be shown all the fliers that are PaymentPending Status")]
        public void ThenIWilloBeShownAllTheFliersThatArePaymentPendingStatus()
        {
            var paymentPendingResult = SpecUtil.ControllerResult as ViewResult;
            var paymentPendingModel = paymentPendingResult.Model as List<BulletinFlierModel>;
            Assert.IsTrue(paymentPendingModel.Count() == 1);
            Assert.AreEqual(paymentPendingModel.First().Title, "This is a Title");
            Assert.AreEqual(paymentPendingModel.First().PendingCredits, 80);
        }
    }
}
