using System;
using System.Linq;
using Website.Infrastructure.Query;

namespace Website.Domain.Browser.Query
{
    public interface QueryServiceForBrowserAggregateInterface : GenericQueryServiceInterface 
    {
        IQueryable<string> GetEntityIdsByBrowserId<EntityType>(String browserId) where EntityType : class, BrowserIdInterface, new();
    }

    public static class QueryServiceForBrowserAggregateInterfaceExtensions
    {
        public static IQueryable<EntityType> GetByBrowserId<EntityType>(
            this QueryServiceForBrowserAggregateInterface browserQueryService, String browserId) 
            where EntityType : class, BrowserIdInterface, new()
        {
            return browserQueryService.GetEntityIdsByBrowserId<EntityType>(browserId)
                .Select(id => browserQueryService.FindById<EntityType>(id));
        }
    }
}
