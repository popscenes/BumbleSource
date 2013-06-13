using Website.Domain.Location;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Query
{
    public class FindFlyersByLocationAndDistanceQuery : QueryInterface
    {
        public Location Location { get; set; }

        public int Distance { get; set; }

        public int Take { get; set; }
    }
}