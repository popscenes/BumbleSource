﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.17929
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace PostaFlya.Specification.Browsers
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Payment")]
    public partial class PaymentFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Payment", "In order to creater fliers with extra features\r\nAs a browser\r\nI want to be pay to" +
                    " attach these features to my fliers", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Accouunt Credit Page")]
        [NUnit.Framework.CategoryAttribute("mytag")]
        public virtual void AccouuntCreditPage()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Accouunt Credit Page", new string[] {
                        "mytag"});
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I am a BROWSER in PARTICIPANT ROLE", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.When("I go to the Add ACCOUUNT CREDIT PAGE", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("I will be presented with the valid PAYMENT OPTIONS", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Add Credit TO Account")]
        public virtual void AddCreditTOAccount()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Add Credit TO Account", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I Am on the Add ACCOUUNT CREDIT PAGE", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.When("I go Select a PAYMENT OPTION", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("I will be redirected to that OPTIONS PROCESS", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Payment Callback Success")]
        public virtual void PaymentCallbackSuccess()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Payment Callback Success", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I Have Selected a PAYMENT OPTION", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.When("The Payment OPTION is Completed Successfully", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("I will be Shown the Transaction Details", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            testRunner.And("the my account will have the credit i purchased", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Payment Callback Failure")]
        public virtual void PaymentCallbackFailure()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Payment Callback Failure", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I Have Selected a PAYMENT OPTION", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.When("The Payment OPTION is Completed Unsuccessfully", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("I will be Shown the Error Details", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            testRunner.And("the my account will not have the credit i purchased", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Payment Transaction History")]
        public virtual void PaymentTransactionHistory()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Payment Transaction History", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I have a Successful PAYMENT TRANSACTION", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.And("I have a Unuccessful PAYMENT TRANSACTION", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            testRunner.When("I navigate to the TRANSACTION HISTORY PAGE", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("I will be presented with My Transactions", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View PaymentPending Fliers")]
        public virtual void ViewPaymentPendingFliers()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View PaymentPending Fliers", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I am a BROWSER in PARTICIPANT ROLE", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.And("I Create Flier With With Insufficient Credit", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            testRunner.When("I navigate to the Pendng Fliers Page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("I will be shown all the fliers that are PaymentPending Status", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Pay For Pending Fliers")]
        public virtual void PayForPendingFliers()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Pay For Pending Fliers", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I am a BROWSER in PARTICIPANT ROLE", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.And("I Create Flier With With Insufficient Credit", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            testRunner.When("I Add Credit To My Account", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.And("I navigate to the Pendng Fliers Page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            testRunner.And("I Choose to pay for a flier", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            testRunner.Then("I will no longer have fliers that are PaymentPending Status", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
