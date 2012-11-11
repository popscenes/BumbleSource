﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using NUnit.Framework;
using PostaFlya.Controllers;
using PostaFlya.Domain.Flier;
using PostaFlya.Models;
using PostaFlya.Models.Flier;
using PostaFlya.Specification.Util;
using TechTalk.SpecFlow;
using Website.Application.Payment;

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
            _flier.GivenABrowserHasNavigatedToTheCreateFlierPage("Default");
            _flier.AndIChooseToAttachMyDefaultContactDetails();
            _flier.WhenISubmitTheRequiredDataForAFlier();
        }

        [When(@"I go to the FLIER PAYMENT PAGE")]
        public void WhenIGoToTheFLIERPAYMENTPAGE()
        {
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var paymentController = SpecUtil.GetController<PaymentController>();
            SpecUtil.ControllerResult = paymentController.FlierPayment(flier.Id);
        }

        [Then(@"I will be presented with the valid PAYMENT OPTIONS")]
        public void ThenIWillBePresentedWithTheValidPAYMENTOPTIONS()
        {
            var controllerResult = SpecUtil.ControllerResult as ViewResult;
            var flierPaymnetsModel = controllerResult.Model as FlierPaymentModel;

            Assert.AreEqual(flierPaymnetsModel.PaymentOptions.Count, 2);
        }

        [Then(@"the FLIER COST")]
        public void ThenTheFLIERCOST()
        {
            var controllerResult = SpecUtil.ControllerResult as ViewResult;
            var flierPaymnetsModel = controllerResult.Model as FlierPaymentModel;
            Assert.AreEqual(flierPaymnetsModel.FlierCost, 2.00);
        }

        [Given(@"I Am on the FLIER PAYMENT PAGE")]
        public void GivenIAmOnTheFLIERPAYMENTPAGE()
        {

            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var paymentController = SpecUtil.GetController<PaymentController>();
        }

        [When(@"I go Select a PAYMENT OPTION")]
        public void WhenIGoSelectAPAYMENTOPTION()
        {
            var flier = ScenarioContext.Current["flier"] as FlierInterface;
            var paymentController = SpecUtil.GetController<PaymentController>();
            SpecUtil.ControllerResult = paymentController.PaymentProcess(flier.Id, "Flier", 2.00);
        }

        [Then(@"I will be redirected to that OPTIONS PROCESS")]
        public void ThenIWillBeRedirectedToThatOPTIONSPROCESS()
        {
            var controllerResult = SpecUtil.ControllerResult as RedirectResult;
            Assert.AreEqual(controllerResult.Url, "http://testpayment.com/");
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
            SpecUtil.ControllerResult = paymentController.PaymnetCallback("paymnetId", "PayerId", -1, 0.00, "Test Error Message");
        }

        [Then(@"I will be Shown the Error Details")]
        public void ThenIWillBeShownTheErrorDetails()
        {
            var controllerResult = SpecUtil.ControllerResult as ViewResult;
            var flierPaymentResult = controllerResult.Model as FlierPaymentResult;
            Assert.AreEqual(flierPaymentResult.PaymentMessage, "Test Error Message");
        }

        [When(@"The Payment OPTION is Completed Successfully")]
        public void WhenThePaymentOPTIONIsCompletedSuccessfully()
        {
            var paymentController = SpecUtil.GetController<PaymentController>();
            SpecUtil.ControllerResult = paymentController.PaymnetCallback("test1", "test2", 1, 2.00, "Success");
        }

        [Then(@"I will be Shown the Transaction Details")]
        public void ThenIWillBeShownTheTransactionDetails()
        {
            var controllerResult = SpecUtil.ControllerResult as ViewResult;
            var flierPaymentResult = controllerResult.Model as FlierPaymentResult;
            Assert.AreEqual(flierPaymentResult.PaymentMessage, "Success");
            Assert.AreEqual(flierPaymentResult.Transaction.PayerId, "test2");
            Assert.AreEqual(flierPaymentResult.Transaction.TrandactionId, "test1");
            Assert.AreEqual(flierPaymentResult.Transaction.Type, "flier");
            Assert.AreEqual(flierPaymentResult.Transaction.EntityId, 1);
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
            var transactionList = controllerResult.Model as List<PaymentTransaction>;
            Assert.AreEqual(transactionList.Count, 2);
        }
    }
}