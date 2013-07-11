using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Infrastructure.Domain;

namespace Website.Infrastructure.Query
{
    public class FindByFriendlyIdQuery<QueryForType> : QueryInterface<QueryForType>
        where QueryForType : class, AggregateRootInterface, new()
    {
        public string FriendlyId { get; set; }
    }
}
