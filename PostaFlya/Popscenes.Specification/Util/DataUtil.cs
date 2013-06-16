using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Types;
using NUnit.Framework;
using PostaFlya.DataRepository.Search.SearchRecord;
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
    }
}
