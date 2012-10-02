﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
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
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "1.9.0.77")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [MbUnit.Framework.TestFixtureAttribute()]
    [MbUnit.Framework.DescriptionAttribute("As a PARTICIPANT with IdentityVerified role \r\nI want to be able to bid on a TASKJ" +
        "OB \r\nso that I am eligible for ASSIGNMENT to that TASKJOB")]
    public partial class BiddingOnATaskFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        [MbUnit.Framework.FixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "bidding on a task", "As a PARTICIPANT with IdentityVerified role \r\nI want to be able to bid on a TASKJ" +
                    "OB \r\nso that I am eligible for ASSIGNMENT to that TASKJOB", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [MbUnit.Framework.DescriptionAttribute("Navigate To TaskJob Bid")]
        public virtual void NavigateToTaskJobBid()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Navigate To TaskJob Bid", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("there is a flier with TASKJOB behaviour", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.Then("I should be able to navigate to the BID page for that TASKJOB", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
        
        [MbUnit.Framework.TestAttribute()]
        [MbUnit.Framework.DescriptionAttribute("TaskJob Bid")]
        public virtual void TaskJobBid()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("TaskJob Bid", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I have navigated to the BID page for a TASKJOB", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.When("I place a TASKBID on the TASKJOB", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("the TASKBID will be registered against the TASKJOB", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
