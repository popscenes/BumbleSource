using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Popscenes.Specification.Util;
using TechTalk.SpecFlow;

namespace Popscenes.Specification.WebApi.Location
{
    [Binding]
    public class LocationHooks : SetupBase
    {
        // For additional details on SpecFlow hooks see http://go.specflow.org/doc-hooks

        [BeforeScenario("locationfeatures")]
        public override void BeforeScenario()
        {
            base.BeforeScenario();
        }

        [AfterScenario("locationfeatures")]
        public override void AfterScenario()
        {
            base.AfterScenario();
        }

        [AfterStep("locationfeatures")]
        public override void AfterStep()
        {
            base.AfterStep();
        }
    }
}
