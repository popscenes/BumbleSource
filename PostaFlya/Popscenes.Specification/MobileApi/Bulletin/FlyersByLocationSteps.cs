using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;

namespace Popscenes.Specification.MobileApi.Bulletin
{
    [Binding]
    public class FlyersByLocationSteps : Steps
    {
        [Given(@"There are (.*) flyers within (.*) kilometers of the geolocation (.*), (.*)")]
        public void GivenThereAreFlyersAroundTheGeolocation(int flyercount, int kilometers, string latitude, string longitude)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"The content should contain a list of (.*) flyers within (.*) kilometers of (.*), (.*)")]
        public void ThenTheContentShouldContainAListOfFlyersWithinKilometersOf(int takenumber, string kilometers, string latitude, string longitude)
        {
            ScenarioContext.Current.Pending();
        }

    }
}
