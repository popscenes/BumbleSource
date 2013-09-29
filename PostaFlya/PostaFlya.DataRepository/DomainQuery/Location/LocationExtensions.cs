using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Types;
using Website.Azure.Common.Sql;
using Website.Azure.Common.Sql.Infrastructure;
using Website.Domain.Location;

namespace PostaFlya.DataRepository.DomainQuery.Location
{
    public static class LocationExtensions
    {
        public static BoundingBox BoundingBoxFromBuffer(this SqlGeography sqlLoc, int kilometers)
        {
            sqlLoc = sqlLoc.BufferWithTolerance(kilometers * 1000, 0.5, false);
            double latmin = 200;
            double latmax = -200;
            double longmin = 200;
            double longmax = -200;

            var cnt = sqlLoc.STNumPoints();
            for (var i = 0; i < cnt; i++)
            {
                var point = sqlLoc.STPointN(i + 1);
                if (point.Lat < latmin)
                    latmin = point.Lat.Value;
                if (point.Long < longmin)
                    longmin = point.Long.Value;
                if (point.Lat > latmax)
                    latmax = point.Lat.Value;
                if (point.Long > longmax)
                    longmax = point.Long.Value;
            }

            return new BoundingBox()
            {
                Min = new Website.Domain.Location.Location(longmin, latmin),
                Max = new Website.Domain.Location.Location(longmax, latmax)
            };
        }

        public static SqlGeography ToGeography(this LocationInterface location)
        {
            try
            {
                return SqlGeography.Point(location.Latitude, location.Longitude, SqlExecute.Srid);
            }
            catch (Exception e)
            {
                Trace.TraceError("SqlGeography ToGeography failed: {0}\n {0}", e.Message, e.StackTrace);
                return null;
            }
        }

        public static Website.Domain.Location.Location ToLocation(this SqlGeography sqlLoc)
        {
            return new Website.Domain.Location.Location(sqlLoc.Long.Value, sqlLoc.Lat.Value);
        }

        public static SqlGeography ToGeography(this BoundingBox boundingBox)
        {
            try
            {
                var min = SqlGeography.Point(boundingBox.Min.Latitude, boundingBox.Min.Longitude, SqlExecute.Srid);
                var max = SqlGeography.Point(boundingBox.Max.Latitude, boundingBox.Max.Longitude, SqlExecute.Srid);
                return min.STUnion(max);
            }
            catch (Exception e)
            {
                Trace.TraceError("SqlGeography ToGeography failed: {0}\n {0}", e.Message, e.StackTrace);

                return null;//invalid location
            }
        }
    }
}
