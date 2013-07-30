using System;
using System.Linq;
using System.Web.Helpers;
using NUnit.Framework;
using Popscenes.Specification.Util;
using PostaFlya.Areas.MobileApi.Flyers.Model;
using TechTalk.SpecFlow;
using Website.Common.ApiInfrastructure.Model;
using Website.Domain.Location;
using Website.Infrastructure.Util.Extension;

namespace Popscenes.Specification.MobileApi.Bulletin
{
    [Binding]
    public class FlyersByLocationSteps : Steps
    {
        [Given(@"There are (.*) flyers within (.*) kilometers of the geolocation (.*), (.*) with dates starting from (.*)")]
        public void GivenThereAreFlyersAroundTheGeolocation(int flyercount, int kilometers, double latitude, double longitude, string dateString)
        {
            var date = Json.Decode<DateTimeOffset>(dateString.Quoted());
            var boardBuild = DataUtil.GetSomeBoardsAroundTheGeolocation(flyercount, kilometers, latitude, longitude);
            var boards = boardBuild.Build().ToList();
            StorageUtil.StoreAll(boards);
            var flyersbuild = DataUtil.GetSomeFlyersForTheBoards(flyercount, boards, date);
            var flyers = flyersbuild.Build();

            StorageUtil.StoreAll(flyers);
        }

        [Then(@"The content should contain a list of flyers within (.*) kilometers of (.*), (.*) in the date range (.*) to (.*)")]
        public void ThenTheContentShouldContainAListOfFlyersWithinKilometersOf(int kilometers, double latitude, double longitude
            , string startdateString, string enddateString)
        {
            var startdate = Json.Decode<DateTimeOffset>(startdateString.Quoted());
            var enddate = Json.Decode<DateTimeOffset>(enddateString.Quoted());
            var content = SpecUtil.GetResponseContentAs<ResponseContent<FlyersByDateContent>>();
            Assert.That(content, Is.Not.Null);
            Assert.That(content.Data, Is.Not.Null);
            Assert.That(content.Data.Flyers, Is.Not.Null);
            Assert.That(content.Data.Flyers.Count, Is.GreaterThan(0));

            var targetLoc = new Location(longitude, latitude);
            foreach (var loc in content.Data.Flyers.Select(model => model.Value.VenueBoard.Location))
            {
                DataUtil.AssertIsWithinKmsOf(targetLoc, kilometers, loc.ToDomainModel());
            }

            var prev = default(DateTimeOffset);
            foreach (var curr in content.Data.Dates
                .Select(f => f.Date).Where(curr => prev != default(DateTimeOffset)))
            {
                Assert.That(prev, Is.GreaterThanOrEqualTo(curr));
                prev = curr;
                Assert.That(prev, Is.GreaterThanOrEqualTo(startdate));
                Assert.That(prev, Is.LessThan(enddate));
            }

            foreach (var flierid in content.Data.Dates.SelectMany(date => date.FlyerIds))
            {
                Assert.True(content.Data.Flyers.ContainsKey(flierid));
            }
        }

//        [Given(@"I have retrieved the first (.*) flyers within (.*) kilometers of (.*), (.*) using (.*)")]
//        public void GivenIHaveRetrievedTheFirstFlyersWithinKilometersOf(int takenumber, int kilometers, double latitude, double longitude, string requestFormat)
//        {
//            When(string.Format(@"I perform a get request for the path " + requestFormat, latitude, longitude, kilometers, takenumber));
//            Then(@"I should receive a http response with a status of 200");
//            Then(@"The content should have a response status of OK");
//            Then(string.Format(@"The content should contain a list of {0} flyers within {1} kilometers of {2}, {3}", takenumber, kilometers, latitude, longitude));
//        }
//
//        [When(@"I attempt to retrieve the next (.*) flyers within (.*) kilometers of (.*), (.*) using (.*)")]
//        public void WhenIAttemptToRetrieveTheNextFlyersWithinKilometersOf(int takenumber, int kilometers, double latitude, double longitude, string requestFormat)
//        {
//            var last = SpecUtil.GetResponseContentAs<ResponseContent<FlyersByDateContent>>().Data.Flyers.Last();
//            SpecUtil.GetRequest(string.Format(requestFormat, latitude, longitude, kilometers, takenumber, last.Id));
//        }


    }
}
