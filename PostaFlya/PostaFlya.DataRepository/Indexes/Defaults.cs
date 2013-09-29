using System;
using PostaFlya.DataRepository.DomainQuery.Location;
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
