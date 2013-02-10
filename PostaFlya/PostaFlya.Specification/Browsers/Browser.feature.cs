﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:1.9.0.77
//      SpecFlow Generator Version:1.9.0.0
//      Runtime Version:4.0.30319.18034
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
    [NUnit.Framework.DescriptionAttribute("Login")]
    public partial class LoginFeature
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Login", "As a Participant\r\nI want to be able to log onto the site \r\nso that I am Authentic" +
                    "ated", ProgrammingLanguage.CSharp, ((string[])(null)));
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
        [NUnit.Framework.DescriptionAttribute("Identity Provider Authentication Request")]
        [NUnit.Framework.CategoryAttribute("mytag")]
        public virtual void IdentityProviderAuthenticationRequest()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Identity Provider Authentication Request", new string[] {
                        "mytag"});
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("i have navigated to the log on page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.When("i provide Select an Identity Provider", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("i will be Redirected to the Identity Providers Login Page", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Identity Provider Authentication Response")]
        public virtual void IdentityProviderAuthenticationResponse()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Identity Provider Authentication Response", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("i have recieve a resonse from an Identity Provider", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.Then("My credentials will be used to log me in", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Account Create")]
        public virtual void AccountCreate()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Account Create", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("i am not yet a BROWSER with PARTICIPANT ROLE", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.When("i provide correct CREDENTIALS", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("a BROWSER with PARTICIPANT ROLE will be created for me", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Switch to existing BROWSER on Login")]
        public virtual void SwitchToExistingBROWSEROnLogin()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Switch to existing BROWSER on Login", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("i am an existing BROWSER with PARTICIPANT ROLE", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.And("i am currently operating in a BROWSER with TEMPORARY ROLE", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            testRunner.When("i provide correct CREDENTIALS", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("my registered BROWSER will be loaded as the ACTIVE BROWSER", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("update personal details")]
        public virtual void UpdatePersonalDetails()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("update personal details", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I am a BROWSER in PARTICIPANT ROLE", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Name",
                        "FirstName",
                        "MiddleNames",
                        "Surname",
                        "Email",
                        "Address",
                        "Avatar",
                        "WebSite"});
            table1.AddRow(new string[] {
                        "User",
                        "FirstName",
                        "M",
                        "LastName",
                        "user@email.com",
                        "-37.769:144.979",
                        "8F68AE77-0F61-4BFD-92AC-BFCA1CC5B9E2",
                        "http://test.com"});
            testRunner.When("I update my profile details with the following data:", ((string)(null)), table1, "When ");
            testRunner.Then("the profile details will be stored against my browser", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Browser Verifies Identity")]
        public virtual void BrowserVerifiesIdentity()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Browser Verifies Identity", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("I am a PARTICIPANT without IdentityVerified ROLE", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.When("I verify my physical identity", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("I will have IdentityVerified ROLE", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Profile View")]
        public virtual void ProfileView()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Profile View", ((string[])(null)));
            this.ScenarioSetup(scenarioInfo);
            testRunner.Given("There is an existing BROWSER with PARTICIPANT ROLE", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            testRunner.When("i navigate to the public profile view the existing BROWSER", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
            testRunner.Then("i will see the existing BROWSERS fliers and tear off claims", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
