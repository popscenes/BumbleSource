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
    [MbUnit.Framework.DescriptionAttribute("As a BROWSER in a PARTICIPANT role\r\nI want to be able to POST a FLIER \r\nso that i" +
        "t can be included in a DYNAMIC BULLETIN BOARD")]
    public partial class PARTICIPANTCanPOSTFLIERFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        [MbUnit.Framework.FixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "PARTICIPANT can POST FLIER", "As a BROWSER in a PARTICIPANT role\r\nI want to be able to POST a FLIER \r\nso that i" +
                    "t can be included in a DYNAMIC BULLETIN BOARD", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [MbUnit.Framework.DescriptionAttribute("Create Flier With Default Behaviour")]
        public virtual void CreateFlierWithDefaultBehaviour()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create Flier With Default Behaviour", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("i have navigated to the CREATE PAGE for a FLIER TYPE Default");
            testRunner.When("I SUBMIT the required data for a FLIER");
            testRunner.Then("the new FLIER will be created for behviour Default");
            testRunner.And("the FLIER STATUS will be ACTIVE");
            this.ScenarioCleanup();
        }
        
        [MbUnit.Framework.TestAttribute()]
        [MbUnit.Framework.DescriptionAttribute("Edit Flier")]
        public virtual void EditFlier()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Edit Flier", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I have created a FLIER");
            testRunner.When("I navigate to the edit page for that FLIER and update any of the required data fo" +
                    "r a FLIER");
            testRunner.Then("the FLIER will be updated to reflect those changes");
            this.ScenarioCleanup();
        }
        
        [MbUnit.Framework.TestAttribute()]
        [MbUnit.Framework.DescriptionAttribute("Attach Images to an existing flier")]
        public virtual void AttachImagesToAnExistingFlier()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Attach Images to an existing flier", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I have created a FLIER");
            testRunner.When("I add images to the FLIER");
            testRunner.Then("The FLIER will contain the extra images");
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
