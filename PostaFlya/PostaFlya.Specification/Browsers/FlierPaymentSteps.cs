using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using NUnit.Framework;
using Ninject;
using PostaFlya.Controllers;
using PostaFlya.Domain.Flier;
using PostaFlya.Models;
using PostaFlya.Models.Flier;
using PostaFlya.Specification.Util;
using TechTalk.SpecFlow;
using Website.Application.Payment;
using Website.Domain.Payment;
using Website.Infrastructure.Query;

namespace Website.Domain.Payment
{
}

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
            //_flier.AndIChooseToAttachMyDefaultContactDetails();
            _flier.WhenISubmitTheRequiredDataForAFlier();
        }


        [Then(@"I will be presented with the valid PAYMENT OPTIONS")]
        public void ThenIWillBePresentedWithTheValidPAYMENTOPTIONS()
        {
            var controllerResult = SpecUtil.ControllerResult as ViewResult;
            var flierPaymnetsModel = controllerResult.Model as FlierPaymentModel;

            Assert.AreEqual(flierPaymnetsModel.PaymentOptions.Count, 2);
        }

        [When(@"I go Select a PAYMENT OPTION")]
        public void WhenIGoSelectAPAYMENTOPTION()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();

            var paymentController = SpecUtil.GetController<PaymentController>();
            SpecUtil.ControllerResult = paymentController.PaymentProcess(browserInformation.Browser.Id, "Account Credit", 10.00);
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

            SpecUtil.ControllerResult = paymentController.PaymnetCallback("test1", browserInformation.Browser.Id, browserInformation.Browser.Id, 0.00, "Test Error Message");
        }

        [Then(@"I will be Shown the Error Details")]
        public void ThenIWillBeShownTheErrorDetails()
        {
            var controllerResult = SpecUtil.ControllerResult as ViewResult;
            var flierPaymentResult = controllerResult.Model as FlierPaymentResult;
            Assert.AreEqual(flierPaymentResult.PaymentMessage, "Test Error Message");

            Assert.AreEqual(flierPaymentResult.Transaction.PayerId, "40D8AC2A-F95C-40A8-9A75-EE87146838A2");

            Assert.AreEqual(flierPaymentResult.Transaction.Type, "Account Credit");
            Assert.AreEqual(flierPaymentResult.Transaction.PaymentEntityId, "40D8AC2A-F95C-40A8-9A75-EE87146838A2");
            Assert.AreEqual(flierPaymentResult.Transaction.Status, PaymentTransactionStatus.Fail);
        }

        [When(@"The Payment OPTION is Completed Successfully")]
        public void WhenThePaymentOPTIONIsCompletedSuccessfully()
        {
            var paymentController = SpecUtil.GetController<PaymentController>();
            var browserInformation = SpecUtil.GetCurrBrowser();
            SpecUtil.ControllerResult = paymentController.PaymnetCallback("test1", browserInformation.Browser.Id, browserInformation.Browser.Id, 10.00, "");
        }

        [Then(@"I will be Shown the Transaction Details")]
        public void ThenIWillBeShownTheTransactionDetails()
        {
            var controllerResult = SpecUtil.ControllerResult as ViewResult;
            var flierPaymentResult = controllerResult.Model as FlierPaymentResult;
            Assert.IsTrue(string.IsNullOrEmpty(flierPaymentResult.PaymentMessage));
            Assert.AreEqual(flierPaymentResult.Transaction.PayerId, "40D8AC2A-F95C-40A8-9A75-EE87146838A2");
            
            Assert.AreEqual(flierPaymentResult.Transaction.Type, "Account Credit");
            Assert.AreEqual(flierPaymentResult.Transaction.PaymentEntityId, "40D8AC2A-F95C-40A8-9A75-EE87146838A2");
            Assert.AreEqual(flierPaymentResult.Transaction.Status, PaymentTransactionStatus.Success);
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
            Assert.AreEqual(browserInformation.Browser.AccountCredit, 10.00);
        }

        [Then(@"the my account will not have the credit i purchased")]
        public void ThenTheMyAccountWillNotHaveTheCreditIPurchased()
        {
            var browserInformation = SpecUtil.GetCurrBrowser();
            Assert.AreEqual(browserInformation.Browser.AccountCredit, 0.00);
        }

    }
}
