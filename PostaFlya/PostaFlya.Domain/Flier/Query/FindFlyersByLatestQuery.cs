using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Query
{
    public class FindFlyersByLatestQuery : QueryInterface<Flier>
    {
        public int Take { get; set; }

        public string Skip { get; set; }
    }
}