using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using PostaFlya.DataRepository.Search.SearchRecord;
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

            for (var i = 0; i < geogBound.STNumPoints(); i++)
            {
                var point = geog.STPointN(i + 1);
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

        public static IOperable<Flier> GetSomeFlyersForTheBoards(int flyercount, List<Board> venues)
        {
            var counter = flyercount;
            return
            Builder<Flier>.CreateListOfSize(flyercount)
                          .All()
                          .With(flier => flier.Id = Guid.NewGuid().ToString())
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

        public static ISingleObjectBuilder<Flier> GetAFlyer(Guid id, Board board, FlierStatus status = FlierStatus.Active)
        {

            var venue = Builder<VenueInformation>.CreateNew()
                                                  .With(information => information.Address = new Location(50,50))
                                                  .Build();
            return
            Builder<Flier>.CreateNew()
                          .With(flier => flier.Id = id.ToString())
                          .With(flier => flier.LocationRadius = 0)
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
    }
}
