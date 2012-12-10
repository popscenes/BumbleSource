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
namespace PostaFlya.Specification.DynamicBulletinBoard
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("ClaimTearOff")]
    public partial class ClaimTearOffFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "ClaimTearOff", "As a BROWSER\r\nI want to be able to claim a tear off\r\nSo that my claim will be rec" +
                    "orded", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Claim An Initial Tear Off")]
        public virtual void ClaimAnInitialTearOff()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Claim An Initial Tear Off", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I have navigated to the public view page for a FLIER With TEAR OFF", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.When("I claim a tear off for that FLIER", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("I will be recorded as having claimed the flier once", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            testRunner.And("the number of claims against the FLIER will be incremented", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Claim An Initial Tear Off And Send Contact Details")]
        public virtual void ClaimAnInitialTearOffAndSendContactDetails()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Claim An Initial Tear Off And Send Contact Details", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I have navigated to the public view page for a FLIER With TEAR OFF And USER CONTA" +
                    "CT", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.And("The Flier Creator Has 1000 Account Credits", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            testRunner.When("I claim a tear off for that FLIER and send my contact details", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("I will be recorded as having claimed the flier once", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            testRunner.And("the number of claims against the FLIER will be incremented", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            testRunner.And("the Claim will be recorded as having My Contact Details", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            testRunner.And("500 will be deducted from the Flier Creators Account", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Claim A Tear Off When One Has Been Claimed")]
        public virtual void ClaimATearOffWhenOneHasBeenClaimed()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Claim A Tear Off When One Has Been Claimed", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I have navigated to the public view page for a FLIER With TEAR OFF", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.And("I have already claimed a tear off for that FLIER", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            testRunner.When("Another Browser claims a tear off for that FLIER", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("the number of claims against the FLIER will be incremented", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Cant Claim Two Tear Offs  Flier")]
        public virtual void CantClaimTwoTearOffsFlier()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Cant Claim Two Tear Offs  Flier", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I have navigated to the public view page for a FLIER With TEAR OFF", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.And("I have already claimed a tear off for that FLIER", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            testRunner.When("I claim a tear off for that FLIER", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("I will be recorded as having claimed the flier once", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            testRunner.And("the FLIER tear off claims will remain the same", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("View Flier Claims")]
        public virtual void ViewFlierClaims()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View Flier Claims", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I have navigated to the public view page for a FLIER With TEAR OFF", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.And("Someone has claimed a tear off for a FLIER", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            testRunner.Then("I should see the claimed tear offs for the FLIER", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Tear Off Claim Publishes Tear Off Notification")]
        [NUnit.Framework.CategoryAttribute("TearOffNotification")]
        public virtual void TearOffClaimPublishesTearOffNotification()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Tear Off Claim Publishes Tear Off Notification", new string[] {
                        "TearOffNotification"});
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I have navigated to the public view page for a FLIER", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.When("I claim a tear off for that FLIER", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("A Notification for that Tear Off should be published", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
