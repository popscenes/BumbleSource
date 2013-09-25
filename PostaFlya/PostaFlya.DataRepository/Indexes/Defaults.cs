using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostaFlya.DataRepository.Search.SearchRecord;
using Website.Domain.Location;

namespace PostaFlya.DataRepository.Indexes
{
    public static class Defaults
    {
        public const int DefaultNearByIndex = 15;
        public static readonly Func<LocationInterface, LocationInterface, double> Distance = (a, b) => 
            a.ToGeography().STDistance(b.ToGeography()).Value;
    }
}
