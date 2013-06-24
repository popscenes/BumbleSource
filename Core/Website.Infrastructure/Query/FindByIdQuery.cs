using System.Collections.Generic;

namespace Website.Infrastructure.Query
{
    public class FindByIdQuery : QueryInterface
    {
        public string Id { get; set; }
    }

    public class FindByIdsQuery : QueryInterface
    {
        public IEnumerable<string> Ids { get; set; }
    }
}