﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using NUnit.Framework;
using Ninject;
using PostaFlya.Controllers;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;
using PostaFlya.Models;
using PostaFlya.Models.Browser;
using PostaFlya.Models.Flier;
using PostaFlya.Specification.Util;
using TechTalk.SpecFlow;
using Website.Domain.Payment;

namespace PostaFlya.Specification.Fliers
{
    [Binding]
    class FlierPaymentSteps
    {
        private readonly CommonSteps _common = new CommonSteps();
        private readonly FlierSteps _flier = new FlierSteps();

        [Given(@"I have a FLIER that requires payment")]
        public void GivenIHaveAFLIERThatRequiresPayment()
        {
            _flier.GivenIOrAnotherBrowserHasNavigatedToTheCreateFlierPage("Default");
            //_flier.AndIChooseToAttachMyDefaultContactDetails();
            _flier.WhenISubmitTheRequiredDataForAFlier();
        }


        [Then(@"I will be presented with the valid PAYMENT OPTIONS")]
        public void ThenIWillBePresentedWithTheValidPAYMENTOPTIONS()
        {
            var controllerResult = SpecUtil.ControllerResult as ViewResult;
            var flierPaymentsModel = controllerResult.Model as FlierPaymentModel;

            Assert.AreEqual(flierPaymentsModel.PaymentOptions.Count, 3);
        }

        [When(@"I go Select a PAYMENT OPTION")]
        public void WhenIGoSelectAPAYMENTOPTION()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();

            var paymentController = SpecUtil.GetController<PaymentController>();
            SpecUtil.ControllerResult = paymentController.PaymentProcess("AccountCredit", 10.00);
        }

        [Then(@"I will be redirected to that OPTIONS PROCESS")]
        public void ThenIWillBeRedirectedToThatOPTIONSPROCESS()
        {
            var controllerResult = SpecUtil.ControllerResult as RedirectResult;
            Assert.AreEqual(controllerResult.Url, "http://test.com/?amt=10");
        }

        [Given(@"I Have Selected a PAYMENT OPTION")]
        public void GivenIHaveSelectedAPAYMENTOPTION()
        {
            WhenIGoSelectAPAYMENTOPTION();
            WhenIGoSelectAPAYMENTOPTION();
            ThenIWillBeRedirectedToThatOPTIONSPROCESS();
        }

        [When(@"The Payment OPTION is Completed Unsuccessfully")]
        public void WhenThePaymentOPTIONIsCompletedUnsuccessfully()
        {
            var paymentController = SpecUtil.GetController<PaymentController>();
            var browserInformation = SpecUtil.GetCurrBrowser();

            var transaction = new PaymentTransaction()
                {
                    TransactionId = "test1",
                    PaymentEntityId = browserInformation.Browser.Id,
                    BrowserId =  browserInformation.Browser.Id,
                    AggregateId = browserInformation.Browser.Id,
                    PayerId = browserInformation.Browser.Id,
                    Amount = 0.00,
                    Type =  PaymentType.AccountCredit,
                    Status = PaymentTransactionStatus.Fail,
                    Message = "Test Error Message",
                    Id = Guid.NewGuid().ToString()
                };

            SpecUtil.ControllerResult = paymentController.PaymentCallback(transaction);
        }

        [Then(@"I will be Shown the Error Details")]
        public void ThenIWillBeShownTheErrorDetails()
        {
            var controllerResult = SpecUtil.ControllerResult as ViewResult;
            var flierPaymentResult = controllerResult.Model as PaymentResult;
            Assert.AreEqual(flierPaymentResult.PaymentMessage, "Test Error Message");

            Assert.AreEqual(flierPaymentResult.TransactionStatus, "Complete");
        }

        [When(@"The Payment OPTION is Completed Successfully")]
        public void WhenThePaymentOPTIONIsCompletedSuccessfully()
        {
            var paymentController = SpecUtil.GetController<PaymentController>();
            var browserInformation = SpecUtil.GetCurrBrowser();

            var transaction = new PaymentTransaction()
            {
                TransactionId = "test1",
                PaymentEntityId = browserInformation.Browser.Id,
                BrowserId = browserInformation.Browser.Id,
                AggregateId = browserInformation.Browser.Id,
                PayerId = browserInformation.Browser.Id,
                Amount = 10.00,
                Type = PaymentType.AccountCredit,
                Status = PaymentTransactionStatus.Success,
                Message = "",
                Id = Guid.NewGuid().ToString()
            };
            SpecUtil.ControllerResult = paymentController.PaymentCallback(transaction);
        }

        [Then(@"I will be Shown the Transaction Details")]
        public void ThenIWillBeShownTheTransactionDetails()
        {
            var controllerResult = SpecUtil.ControllerResult as ViewResult;
            var flierPaymentResult = controllerResult.Model as PaymentResult;
            Assert.IsTrue(!string.IsNullOrEmpty(flierPaymentResult.PaymentMessage));
            
            Assert.AreEqual(flierPaymentResult.TransactionStatus, "Complete");
        }

        [Given(@"I have a Successful PAYMENT TRANSACTION")]
        public void GivenIHaveASuccessfulPAYMENTTRANSACTION()
        {
            GivenIHaveSelectedAPAYMENTOPTION();
            WhenThePaymentOPTIONIsCompletedSuccessfully();
        }

        [Given(@"I have a Unuccessful PAYMENT TRANSACTION")]
        public void GivenIHaveAUnuccessfulPAYMENTTRANSACTION()
        {
            GivenIHaveSelectedAPAYMENTOPTION();
            WhenThePaymentOPTIONIsCompletedUnsuccessfully();
        }

        [When(@"I navigate to the TRANSACTION HISTORY PAGE")]
        public void WhenINavigateToTheTRANSACTIONHISTORYPAGE()
        {
            var paymentController = SpecUtil.GetController<PaymentController>();
            SpecUtil.ControllerResult = paymentController.PaymentTransactions();
        }

        [Then(@"I will be presented with My Transactions")]
        public void ThenIWillBePresentedWithMyTransactions()
        {
            var controllerResult = SpecUtil.ControllerResult as ViewResult;
            var model = controllerResult.Model as PaymentTrasactionPageModel;

            var transactionList = model.Transactions;
            Assert.AreEqual(transactionList.Count, 2);
            Assert.AreEqual(transactionList.Count(_ => _.Status == PaymentTransactionStatus.Success), 1);
            Assert.AreEqual(transactionList.Count(_ => _.Status == PaymentTransactionStatus.Fail), 1);
            Assert.AreEqual(transactionList.First(_ => _.Status == PaymentTransactionStatus.Fail).Message, "Test Error Message");

        }


        [When(@"I go to the Add ACCOUUNT CREDIT PAGE")]
        [Given(@"I Am on the Add ACCOUUNT CREDIT PAGE")]
        public void WhenIGoToTheAddACCOUUNTCREDITPAGE()
        {
            var paymentController = SpecUtil.GetController<PaymentController>();
            SpecUtil.ControllerResult = paymentController.AddAccountCredit();
        }


        [Then(@"the my account will have the credit i purchased")]
        public void ThenTheMyAccountWillHaveTheCreditIPurchased()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            Assert.AreEqual(browserInformation.Browser.AccountCredit, 1000);
        }

        [Then(@"the my account will not have the credit i purchased")]
        public void ThenTheMyAccountWillNotHaveTheCreditIPurchased()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            Assert.AreEqual(browserInformation.Browser.AccountCredit, 0.00);
        }

        [Given(@"I Create Flier With With Insufficient Credit")]
        public void GivenICreateFlierWithWithInsufficientCredit()
        {
            _flier.GivenIOrAnotherBrowserHasNavigatedToTheCreateFlierPage(FlierBehaviour.Default.ToString());
            _common.GivenIHaveAccountCredit(0);
            _flier.WhenISubmitTheRequiredDataForAFlier();
        }

        [When(@"I navigate to the Pendng Fliers Page")]
        public void WhenINavigateToThePendngFliersPage()
        {

            var profileController = SpecUtil.GetApiController<PendingFliersApiController>();
            SpecUtil.ControllerResult = profileController.Get();
        }

        [Then(@"I will be shown all the fliers that are PaymentPending Status")]
        public void ThenIWilloBeShownAllTheFliersThatArePaymentPendingStatus()
        {
            var paymentPendingModel = SpecUtil.ControllerResult as List<BulletinFlierSummaryModel>;
            Assert.IsTrue(paymentPendingModel.Count() == 1);
            Assert.AreEqual(paymentPendingModel.First().Title, "This is a Title");
            Assert.AreEqual(100, paymentPendingModel.First().PendingCredits);
        }

        [When(@"I Add Credit To My Account")]
        public void WhenIAddCreditToMyAccount()
        {
            _common.GivenIHaveAccountCredit(1000);
        }

        [When(@"I Choose to pay for a flier")]
        public void WhenIChooseToPayForAFlier()
        {
            var paymentPendingModel = SpecUtil.ControllerResult as List<BulletinFlierSummaryModel>;
            var profileController = SpecUtil.GetApiController<PendingFliersApiController>();
            var browserInformation = SpecUtil.GetCurrBrowser();
            profileController.Put(browserInformation.Browser.Id, paymentPendingModel.First().Id);
        }

        [Then(@"I will still have fliers that are PaymentPending Status and have the cost of")]
        public void ThenIWillStillHaveFliersThatArePaymentPendingStatusAndHaveTheCostOf()
        {
            var profileController = SpecUtil.GetApiController<PendingFliersApiController>();
            var paymentPendingModel = profileController.Get();
            Assert.AreEqual(paymentPendingModel.First().Title, "This is a Title");
            Assert.AreEqual(600, paymentPendingModel.First().PendingCredits);
        }



        [Then(@"I will no longer have fliers that are PaymentPending Status")]
        public void ThenIWillNoLongerHaveFliersThatArePaymentPendingStatus()
        {
            var profileController = SpecUtil.GetApiController<PendingFliersApiController>();
            var paymentPendingModel = profileController.Get();
            Assert.AreEqual(paymentPendingModel.Count(), 0);
        }


    }
}
