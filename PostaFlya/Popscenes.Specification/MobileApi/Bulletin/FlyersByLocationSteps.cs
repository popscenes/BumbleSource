using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using Microsoft.SqlServer.Types;
using Popscenes.Specification.Util;
using PostaFlya.DataRepository.Search.SearchRecord;
using PostaFlya.Domain.Flier;
using TechTalk.SpecFlow;
using Website.Domain.Location;

namespace Popscenes.Specification.MobileApi.Bulletin
{
    [Binding]
    public class FlyersByLocationSteps : Steps
    {
        [Given(@"There are (.*) flyers within (.*) kilometers of the geolocation (.*), (.*)")]
        public void GivenThereAreFlyersAroundTheGeolocation(int flyercount, int kilometers, double latitude, double longitude)
        {
            var locs = DataUtil.GetSomeRandomLocationsWithKmsOf(flyercount, new Location(longitude, latitude),
                                                                kilometers);
            var flyers = Builder<Flier>.CreateListOfSize(flyercount)
                                       .All()
                                       .With(flier => flier.LocationRadius = 0)
                                       .With(f => f.Location = locs[--flyercount]).Build();
            
            StorageUtil.StoreAll(flyers);

        }

        [Then(@"The content should contain a list of (.*) flyers within (.*) kilometers of (.*), (.*)")]
        public void ThenTheContentShouldContainAListOfFlyersWithinKilometersOf(int takenumber, string kilometers, string latitude, string longitude)
        {
            ScenarioContext.Current.Pending();
        }

    }
}
