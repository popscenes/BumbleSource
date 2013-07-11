using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Website.Domain.Browser.Query
{
    public class GetByBrowserIdQuery<QueryForType> : QueryInterface<QueryForType>, BrowserIdInterface
        where QueryForType : class, BrowserIdInterface, EntityIdInterface, new()
    {
        public string BrowserId { get; set; }
    }
}
