using System.Collections.Generic;
using Website.Infrastructure.Domain;

namespace Website.Infrastructure.Query
{
    public class FindByIdQuery<QueryForType> : QueryInterface<QueryForType>
        where QueryForType : class, AggregateRootInterface, new()
    {
        public string Id { get; set; }
    }

    public class FindByIdsQuery<QueryForType> : QueryInterface<QueryForType>
        where QueryForType : class, AggregateRootInterface, new()
    {
        public IEnumerable<string> Ids { get; set; }
    }

    public class FindByAggregateIdQuery<QueryForType> : QueryInterface<QueryForType>
    where QueryForType : class, AggregateInterface, new()
    {
        public string Id { get; set; }
    }
}