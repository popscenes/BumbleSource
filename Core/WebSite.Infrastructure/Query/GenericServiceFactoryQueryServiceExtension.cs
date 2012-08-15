using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Service;

namespace WebSite.Infrastructure.Query
{
    public static class GenericServiceFactoryQueryServiceExtension
    {
        internal readonly static Type GenericQueryService = typeof(GenericQueryServiceInterface<>);

        public static AsType GetGenericQueryService<AsType>(this GenericServiceFactoryInterface serviceFactory, Type entityinterface) where AsType : class
        {
            return serviceFactory.FindService<AsType>(GenericQueryService, entityinterface);
        }
    }
}
