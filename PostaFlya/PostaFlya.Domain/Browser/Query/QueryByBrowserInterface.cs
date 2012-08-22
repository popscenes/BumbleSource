using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.Browser.Query
{
    public interface QueryByBrowserInterface     
    {
        IQueryable<EntityType> GetByBrowserId<EntityType>(String browserId) where EntityType : class, BrowserIdInterface, new();
    }
}
