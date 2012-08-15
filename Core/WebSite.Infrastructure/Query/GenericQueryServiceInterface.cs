using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;

namespace WebSite.Infrastructure.Query
{

    public interface GenericQueryServiceInterface<out EntityType> : QueryServiceInterface
    {
        new EntityType FindById(string id);
    }
}
