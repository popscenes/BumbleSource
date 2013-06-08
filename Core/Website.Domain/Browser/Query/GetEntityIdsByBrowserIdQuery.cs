using System;
using System.Linq;
using Website.Infrastructure.Query;

namespace Website.Domain.Browser.Query
{
    public class GetEntityIdsByBrowserIdQuery : QueryInterface, BrowserIdInterface
    {
        public string BrowserId { get; set; }
    }

//    public interface GenericQueryServiceInterface : GenericQueryServiceInterface 
//    {
//        IQueryable<string> GetEntityIdsByBrowserId<EntityType>(String browserId) where EntityType : class, BrowserIdInterface, new();
//    }
//
//    public static class GenericQueryServiceInterfaceExtensions
//    {
//        public static IQueryable<EntityType> GetByBrowserId<EntityType>(
//            this GenericQueryServiceInterface browserQueryService, String browserId) 
//            where EntityType : class, BrowserIdInterface, new()
//        {
//            return browserQueryService.GetEntityIdsByBrowserId<EntityType>(browserId)
//                .Select(id => browserQueryService.FindById<EntityType>(id));
//        }
//    }
}
