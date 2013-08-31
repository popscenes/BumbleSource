using System.Collections.Generic;
using Website.Infrastructure.Query;

namespace Website.Domain.Query
{
    public class AutoCompleteByTermsQuery : QueryInterface<SearchEntityRecord>
    {
        public List<string> Terms { get; set; }

    }
}