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
            var locs = DataUtil.GetSomeRandomLocationsWithKmsOf(flyercount, new Location(longitude, latitude),
                                                                kilometers);
            var counter = flyercount;
            var venues = Builder<VenueInformation>.CreateListOfSize(flyercount)
                                                  .All()
                                                  .With(information => information.Address = locs[--counter])
                                                  .Build();
            counter = flyercount;
            var flyers = Builder<Flier>.CreateListOfSize(flyercount)
                                       .All()
                                       .With(flier => flier.Id = Guid.NewGuid().ToString())
                                       .With(flier => flier.LocationRadius = 0)
                                       .With(flier => flier.Venue = venues[--counter])
                                       .With(flier => flier.Status = FlierStatus.Active)
                                       .Build();
            
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
            foreach (var loc in content.Data.Flyers.Select(model => model.Venue.Address))
            {
                DataUtil.AssertIsWithinKmsOf(targetLoc, kilometers, loc.ToDomainModel());
            }

        }

    }
}
