using System.Collections.Generic;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Query
{
    public class FindFlyersByFeaturedQuery : QueryInterface<Flier>
    {
        public int Take { get; set; }

        public string Skip { get; set; }
    }
}