using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Popscenes.Specification.Util;
using TechTalk.SpecFlow;

namespace Popscenes.Specification.MobileApi.Bulletin
{
    [Binding]
    public class BulletinHooks : SetupBase 
    {
        // For additional details on SpecFlow hooks see http://go.specflow.org/doc-hooks

        [BeforeScenario("bulletinfeatures")]
        public override void BeforeScenario()
        {
            base.BeforeScenario();
        }

        [AfterScenario("bulletinfeatures")]
        public override void AfterScenario()
        {
            base.AfterScenario();
        }

        [AfterStep("bulletinfeatures")]
        public override void AfterStep()
        {
            base.AfterStep();
        }
    }
}
