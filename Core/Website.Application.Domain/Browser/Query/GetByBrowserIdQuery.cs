using Website.Domain.Browser;
using Website.Infrastructure.Query;

namespace Website.Application.Domain.Browser.Query
{
    public class GetByBrowserIdQuery : QueryInterface, BrowserIdInterface  
    {
        public string BrowserId { get; set; }
    }
}
