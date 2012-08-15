using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.Browser.Query
{
    public interface QueryByBrowserInterface< out EntityInterfaceType>
        where EntityInterfaceType : BrowserIdInterface
    {
        IQueryable<EntityInterfaceType> GetByBrowserId(String browserId);
    }
}
