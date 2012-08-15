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
namespace PostaFlya.Specification.Behaviour.TaskJob
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.55")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [MbUnit.Framework.TestFixtureAttribute()]
    [MbUnit.Framework.DescriptionAttribute("As a PARTICIPANT with IdentityVerified role \r\nI want to be able to POST a TASK FL" +
        "YA \r\nso that other PARTICIPANTS with TASKFLYA role can BID for that TASK")]
    public partial class PostATASKFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        [MbUnit.Framework.FixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Post a TASK", "As a PARTICIPANT with IdentityVerified role \r\nI want to be able to POST a TASK FL" +
                    "YA \r\nso that other PARTICIPANTS with TASKFLYA role can BID for that TASK", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [MbUnit.Framework.DescriptionAttribute("Create Flier With TaskJob")]
        public virtual void CreateFlierWithTaskJob()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create Flier With TaskJob", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("i have navigated to the CREATE PAGE for a FLIER TYPE TaskJob");
            testRunner.When("I SUBMIT the required data for a FLIER");
            testRunner.Then("the new FLIER will be created for behviour TaskJob");
            testRunner.And("the FLIER STATUS will be PENDING");
            this.ScenarioCleanup();
        }
        
        [MbUnit.Framework.TestAttribute()]
        [MbUnit.Framework.DescriptionAttribute("Create TaskJob Behaviour attached to Flier")]
        public virtual void CreateTaskJobBehaviourAttachedToFlier()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create TaskJob Behaviour attached to Flier", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I am a PARTICIPANT with IdentityVerified ROLE");
            testRunner.And("I have created a FLIER of BEHAVIOUR TaskJob");
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "CostOverhead",
                        "MaxAmount",
                        "ExtraLocations"});
            table1.AddRow(new string[] {
                        "0",
                        "100",
                        ""});
            testRunner.When("I submit the following data for the TASKJOB:", ((string)(null)), table1);
            testRunner.Then("the TASKJOB will be stored for the FLIER");
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
