using Website.Infrastructure.Query;

namespace Website.Domain.Query
{
    public class AutoCompleteByTermsQuery : QueryInterface<SearchEntityRecord>
    {
        public string Terms { get; set; }

    }
}