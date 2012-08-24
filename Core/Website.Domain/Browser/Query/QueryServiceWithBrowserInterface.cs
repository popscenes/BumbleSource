using WebSite.Infrastructure.Query;

namespace Website.Domain.Browser.Query
{
    public interface QueryServiceWithBrowserInterface : 
        GenericQueryServiceInterface, QueryByBrowserInterface
    {
    }
}
