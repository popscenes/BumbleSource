using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;

using Microsoft.SqlServer.Types;
using NUnit.Framework;
using PostaFlya.DataRepository.DomainQuery.Location;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Venue;
using Website.Domain.Location;

namespace Popscenes.Specification.Util
{
    public static class DataUtil
    {
        public static List<Location> GetSomeRandomLocationsWithKmsOf(int numToGet, Location loc, int kilometers)
        {
            SqlGeography geog = loc.ToGeography();

            var geogBound = geog.BufferWithTolerance(kilometers * 1000, 0.2, false);
            double latmin = 200;
            double latmax = -200;
            double longmin = 200;
            double longmax = -200;

            for (var i = 0; i <= geogBound.STNumPoints(); i++)
            {
                var point = geogBound.STPointN(i + 1);
                if (point.Lat < latmin)
                    latmin = point.Lat.Value;
                if (point.Long < longmin)
                    longmin = point.Long.Value;
                if (point.Lat > latmax)
                    latmax = point.Lat.Value;
                if (point.Long > longmax)
                    longmax = point.Long.Value;
            }

            var ret = new List<Location>();
            var r = new Random();
            do
            {
                var lat = r.Next((int) Math.Floor(latmin), (int) Math.Floor(latmax)) + r.NextDouble();
                var lng = r.Next((int) Math.Floor(longmin), (int) Math.Floor(longmax)) + r.NextDouble();
                var next = new Location(lng, lat).ToGeography();
                if(geog.STDistance(next) < kilometers * 1000)
                    ret.Add(next.ToLocation());


            } while (ret.Count < numToGet);
            return ret;
        }

        public static void AssertIsWithinKmsOf(LocationInterface isLoc, int kilometers, LocationInterface ofLoc)
        {
            SqlGeography isGeog = isLoc.ToGeography();
            SqlGeography ofGeog = ofLoc.ToGeography();
            var dist = (double)isGeog.STDistance(ofGeog);
            Assert.That(dist/1000, Is.LessThanOrEqualTo(kilometers));
        }

        public static IOperable<Board> GetSomeBoardsAroundTheGeolocation(int flyercount, int kilometers, double latitude, double longitude,
            BoardTypeEnum boardTypeEnum = BoardTypeEnum.VenueBoard)
        {
            var locs = GetSomeRandomLocationsWithKmsOf(flyercount, new Location(longitude, latitude),
                                                    kilometers);
            var counter = flyercount;
            var venues = Builder<Board>.CreateListOfSize(flyercount)
                                       .All()
                                       .With(v =>
                                             v.InformationSources 
                                             = Builder<VenueInformation>.CreateListOfSize(1)
                                                .All()
                                                .With(
                                                    information =>
                                                    information.Address =
                                                    locs[--counter])
                                                .Build().ToList())
                                        .With(board => board.BoardTypeEnum = boardTypeEnum)
                                        .With(board => board.Id = Guid.NewGuid().ToString());
            return venues;
        }

        public static IOperable<Flier> GetSomeFlyersForTheBoards(int flyercount, List<Board> venues, DateTimeOffset date)
        {
            
            var counter = flyercount;
            return
            Builder<Flier>.CreateListOfSize(flyercount)
                          .All()                         
                          .With(flier => flier.Id = Guid.NewGuid().ToString())
                          .With(flier => flier.EventDates = GetSomeRandomDatesStartingFrom(date, 10))
                          .With(flier => flier.LocationRadius = 0)
                          .With(flier => flier.Boards
                              = Builder<BoardFlier>.CreateListOfSize(1)
                              .All()
                              .With(boardFlier => boardFlier.BoardId = venues[(--counter)%venues.Count].Id)
                              .With(boardFlier => boardFlier.Status = BoardFlierStatus.Approved)                              
                              .Build().ToList()
                            )
                          .With(flier => flier.Status = FlierStatus.Active)
                          .With(flier => flier.CreateDate = DateTime.UtcNow.AddDays(flyercount - counter));
        }

        private static List<DateTimeOffset> GetSomeRandomDatesStartingFrom(DateTimeOffset date, int dayrange = 10)
        {
            var r = new Random();
            var num = r.Next(1, 5);
            var ret = new List<DateTimeOffset>();
            for (int i = 0; i < num; i++)
            {
                ret.Add(date.AddDays(r.Next(dayrange)));
            }
            return ret;
        }

        public static ISingleObjectBuilder<Flier> GetAFlyer(Guid id, Board board, FlierStatus status = FlierStatus.Active)
        {

            var venue = Builder<VenueInformation>.CreateNew()
                                                  .With(information => information.Address = new Location(50,50))
                                                  .Build();
            return
            Builder<Flier>.CreateNew()
                          .With(flier => flier.Id = id.ToString())
                          .With(flier => flier.LocationRadius = 0)
                          .With(flier => flier.EventDates = GetSomeRandomDatesStartingFrom(DateTimeOffset.Now))
                            .With(flier => flier.Boards
                              = Builder<BoardFlier>.CreateListOfSize(1)
                              .All()
                              .With(boardFlier => boardFlier.BoardId = board.Id)
                              .With(boardFlier => boardFlier.Status = BoardFlierStatus.Approved)
                              .Build().ToList()
                            )
                          .With(flier => flier.Status = status)
                          .With(flier => flier.CreateDate = DateTime.UtcNow);
        }

        public static ISingleObjectBuilder<Board> GetABoard(Guid id,
                                                            BoardTypeEnum boardTypeEnum = BoardTypeEnum.VenueBoard,
                                                            BoardStatus status = BoardStatus.Approved)
        {
            return
                Builder<Board>.CreateNew()
                              .With(board => board.Id = id.ToString())
                              .With(board => board.InformationSources
                                             = Builder<VenueInformation>.CreateListOfSize(1)
                                                                        .All()
                                                                        .With(
                                                                            information =>
                                                                            information.Address =
                                                                            new Location(50, 50))
                                                                        .Build().ToList())
                              .With(board => board.Status = status)
                              .With(board => board.BoardTypeEnum = boardTypeEnum);
        }

        public static IOperable<Suburb> GetSomeSuburbs(int count, string wordPrefix = "", int wordPrefixCount = 1, int kilometers = 100, double latitude = -37.769, double longitude = 144.979)
        {
            var locs = GetSomeRandomLocationsWithKmsOf(count, new Location(longitude, latitude),
                                        kilometers);

            var cnt = count;
            var ret = Builder<Suburb>
                .CreateListOfSize(count)
                .All()
                .With(suburb => suburb.Latitude = locs[--cnt].Latitude)
                .With(suburb => suburb.Longitude = locs[cnt].Longitude)
                .TheFirst(wordPrefixCount)
                .With(suburb => suburb.Locality = GetSuburbNameContainng(wordPrefix))
                .All().With(suburb => suburb.Id = suburb.GetGeneratedId());
            return ret;
        }

        private static string GetSuburbNameContainng(string wordPrefix)
        {
            var rand = new Random();
            var wordcnt = rand.Next(1, 3);
            var prefixword = rand.Next(0, wordcnt - 1);
            var ret = "";
            for (var i = 0; i < wordcnt; i++)
            {
                if (ret.Length > 0) ret += " ";
                var chars = (string.IsNullOrWhiteSpace(wordPrefix) || prefixword != i) ? rand.Next(4, 15) : rand.Next(0, 10);
                if (prefixword == i)
                    ret += wordPrefix;
                for (int j = 0; j < chars; j++)
                {
                    ret += (char)('a' + rand.Next(0, 25));
                }

            }
            return ret;
        }
    }
}
