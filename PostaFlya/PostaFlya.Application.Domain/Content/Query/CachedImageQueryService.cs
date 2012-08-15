using System.Linq;
using System.Runtime.Caching;
using WebSite.Application.Binding;
using WebSite.Application.Caching.Query;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Content.Query;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Query;
using WebSite.Infrastructure.Binding;

namespace PostaFlya.Application.Domain.Content.Query
{
    public class CachedImageQueryService : TimedExpiryCachedQueryService,
                                           ImageQueryServiceInterface
    {
        private readonly ImageQueryServiceInterface _imageQueryService;

        public CachedImageQueryService([SourceDataSource]ImageQueryServiceInterface imageQueryService
                , ObjectCache cacheProvider            
                , int defaultSecondsToCache = -1) 
            : base(cacheProvider, CachedImageContext.Region, defaultSecondsToCache)
        {
            _imageQueryService = imageQueryService;
        }

        public ImageInterface FindById(string id)
        {
            //for now don't cache processing status because will
            //be processing images in a separate process most likely
            //if we switch to a shared cache then we can remove this check
            return RetrieveCachedData(
                GetKeyFor(CachedImageContext.Image, id),
                () => _imageQueryService.FindById(id)
                , i => i.Status != ImageStatus.Processing);          
        }

        public IQueryable<ImageInterface> GetByBrowserId(string browserId)
        {
            return RetrieveCachedData(
                GetKeyFor(CachedImageContext.Browser, browserId),
                () => _imageQueryService.GetByBrowserId(browserId).ToList())
                .AsQueryable();
        }

        object QueryServiceInterface.FindById(string id)
        {
            return FindById(id);
        }
    }
}