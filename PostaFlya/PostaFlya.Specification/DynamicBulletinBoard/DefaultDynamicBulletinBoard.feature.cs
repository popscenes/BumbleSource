﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.55
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.269
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
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.55")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [MbUnit.Framework.TestFixtureAttribute()]
    [MbUnit.Framework.DescriptionAttribute("As a BROWSER\r\nI want to view a DYNAMIC BULLETIN BOARD\r\nso that I can see FLIERS")]
    public partial class DefaultDynamicBulletinBoardFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        [MbUnit.Framework.FixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "DefaultDynamicBulletinBoard", "As a BROWSER\r\nI want to view a DYNAMIC BULLETIN BOARD\r\nso that I can see FLIERS", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [MbUnit.Framework.FixtureTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [MbUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [MbUnit.Framework.TearDownAttribute()]
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
        
        [MbUnit.Framework.TestAttribute()]
        [MbUnit.Framework.DescriptionAttribute("ViewDynamicBulletinFliers")]
        public virtual void ViewDynamicBulletinFliers()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("ViewDynamicBulletinFliers", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("there are some TAGS set");
            testRunner.When("I have navigated to the BULLETIN BOARD for a LOCATION");
            testRunner.Then("I should only see FLIERS within a DISTANCE from that LOCATION");
            testRunner.And("I should only see FLIERS with matching TAGS");
            this.ScenarioCleanup();
        }
        
        [MbUnit.Framework.TestAttribute()]
        [MbUnit.Framework.DescriptionAttribute("SetDynamicBulletinDistance")]
        public virtual void SetDynamicBulletinDistance()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("SetDynamicBulletinDistance", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("there are some TAGS set");
            testRunner.And("I have navigated to the BULLETIN BOARD for a LOCATION");
            testRunner.When("I set my DISTANCE");
            testRunner.Then("i should see all fliers within my new DISTANCE that have matching TAGS");
            this.ScenarioCleanup();
        }
        
        [MbUnit.Framework.TestAttribute()]
        [MbUnit.Framework.DescriptionAttribute("ViewDynamicBulletinFliersDefaultOrder")]
        public virtual void ViewDynamicBulletinFliersDefaultOrder()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("ViewDynamicBulletinFliersDefaultOrder", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("there are some TAGS set");
            testRunner.When("I have navigated to the BULLETIN BOARD for a LOCATION");
            testRunner.Then("I should see FLIERS ordered by create date");
            this.ScenarioCleanup();
        }
        
        [MbUnit.Framework.TestAttribute()]
        [MbUnit.Framework.DescriptionAttribute("View FLIER details")]
        public virtual void ViewFLIERDetails()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("View FLIER details", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("There is a FLIER");
            testRunner.When("I have navigated to the public view page for that FLIER");
            testRunner.Then("I should see the public details of that FLIER");
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
