using System;
using System.Linq;

namespace Website.Domain.Browser.Query
{
    public interface QueryByBrowserInterface     
    {
        IQueryable<EntityType> GetByBrowserId<EntityType>(String browserId) where EntityType : class, BrowserIdInterface, new();
    }
}
