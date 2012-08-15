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
namespace PostaFlya.Specification.Fliers
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.55")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [MbUnit.Framework.TestFixtureAttribute()]
    [MbUnit.Framework.DescriptionAttribute("As a BROWSER in PARTICIPANT role\r\nI want to be able to navigate to the my flier p" +
        "age\r\nSo that i can see a list of FLIERS i have created \r\nAnd I can view the deta" +
        "ils of each of those fliers.")]
    public partial class MyFliersPageFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        [MbUnit.Framework.FixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "My Fliers Page", "As a BROWSER in PARTICIPANT role\r\nI want to be able to navigate to the my flier p" +
                    "age\r\nSo that i can see a list of FLIERS i have created \r\nAnd I can view the deta" +
                    "ils of each of those fliers.", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [MbUnit.Framework.DescriptionAttribute("Show Fliers I have Created")]
        [MbUnit.Framework.CategoryAttribute("mytag")]
        public virtual void ShowFliersIHaveCreated()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show Fliers I have Created", new string[] {
                        "mytag"});
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I am a BROWSER in PARTICIPANT ROLE");
            testRunner.When("I navigate to the my fliers page");
            testRunner.Then("I should see a list of fliers I have created");
            this.ScenarioCleanup();
        }
        
        [MbUnit.Framework.TestAttribute()]
        [MbUnit.Framework.DescriptionAttribute("Show Fliers Detail View")]
        public virtual void ShowFliersDetailView()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Show Fliers Detail View", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I am a BROWSER in PARTICIPANT ROLE");
            testRunner.And("I have navigated to the my fliers page");
            testRunner.When("I click on view for my FLIER");
            testRunner.Then("I should see the details for my FLIER");
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
