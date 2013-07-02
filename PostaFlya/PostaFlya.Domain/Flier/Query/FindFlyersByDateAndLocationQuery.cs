using System;
using Website.Domain.Location;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Query
{
    public class FindFlyersByDateAndLocationQuery : QueryInterface<Flier>
    {
        public Location Location { get; set; }

        public int Distance { get; set; }

        public DateTimeOffset Start { get; set; }
        
        public DateTimeOffset End { get; set; }

    }
}