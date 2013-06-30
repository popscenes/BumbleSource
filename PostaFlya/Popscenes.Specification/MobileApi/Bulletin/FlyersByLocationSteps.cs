using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using Popscenes.Specification.Util;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using PostaFlya.Areas.MobileApi.Infrastructure.Model;
using PostaFlya.DataRepository.Search.SearchRecord;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Venue;
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
            var boardBuild = DataUtil.GetSomeBoardsAroundTheGeolocation(flyercount, kilometers, latitude, longitude);
            var boards = boardBuild.Build().ToList();
            StorageUtil.StoreAll(boards);
            var flyersbuild = DataUtil.GetSomeFlyersForTheBoards(flyercount, boards);
            var flyers = flyersbuild.Build();

            StorageUtil.StoreAll(flyers);
        }

        [Then(@"The content should contain a list of (.*) flyers within (.*) kilometers of (.*), (.*)")]
        public void ThenTheContentShouldContainAListOfFlyersWithinKilometersOf(int takenumber, int kilometers, double latitude, double longitude)
        {
            var content = SpecUtil.GetResponseContentAs<ResponseContent<FlyerSummaryContent>>();
            Assert.That(content, Is.Not.Null);
            Assert.That(content.Data, Is.Not.Null);
            Assert.That(content.Data.Flyers, Is.Not.Null);
            Assert.That(content.Data.Flyers.Count, Is.EqualTo(takenumber));

            var targetLoc = new Location(longitude, latitude);
            foreach (var loc in content.Data.Flyers.Select(model => model.VenueBoard.Location))
            {
                DataUtil.AssertIsWithinKmsOf(targetLoc, kilometers, loc.ToDomainModel());
            }

        }

        [Given(@"I have retrieved the first (.*) flyers within (.*) kilometers of (.*), (.*) using (.*)")]
        public void GivenIHaveRetrievedTheFirstFlyersWithinKilometersOf(int takenumber, int kilometers, double latitude, double longitude, string requestFormat)
        {
            When(string.Format(@"I perform a get request for the path " + requestFormat, latitude, longitude, kilometers, takenumber));
            Then(@"I should receive a http response with a status of 200");
            Then(@"The content should have a response status of OK");
            Then(string.Format(@"The content should contain a list of {0} flyers within {1} kilometers of {2}, {3}", takenumber, kilometers, latitude, longitude));
        }

        [When(@"I attempt to retrieve the next (.*) flyers within (.*) kilometers of (.*), (.*) using (.*)")]
        public void WhenIAttemptToRetrieveTheNextFlyersWithinKilometersOf(int takenumber, int kilometers, double latitude, double longitude, string requestFormat)
        {
            var last = SpecUtil.GetResponseContentAs<ResponseContent<FlyerSummaryContent>>().Data.Flyers.Last();
            SpecUtil.GetRequest(string.Format(requestFormat, latitude, longitude, kilometers, takenumber, last.Id));
        }


    }
}
