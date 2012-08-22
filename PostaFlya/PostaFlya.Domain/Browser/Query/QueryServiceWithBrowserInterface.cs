using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Infrastructure.Query;

namespace PostaFlya.Domain.Browser.Query
{
    public interface QueryServiceWithBrowserInterface : 
        GenericQueryServiceInterface, QueryByBrowserInterface
    {
    }
}
