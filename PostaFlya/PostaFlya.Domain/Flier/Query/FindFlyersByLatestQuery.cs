using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Flier.Query
{
    public class FindFlyersByLatestQuery : QueryInterface
    {
        public int Take { get; set; }

        public string Skip { get; set; }
    }
}