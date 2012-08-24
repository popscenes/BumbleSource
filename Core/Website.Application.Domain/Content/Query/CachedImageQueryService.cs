using System.Runtime.Caching;
using WebSite.Infrastructure.Binding;
using Website.Application.Domain.Query;
using Website.Domain.Content.Query;

namespace Website.Application.Domain.Content.Query
{
    public class CachedImageQueryService : CachedQueryServiceWithBrowser,
                                           ImageQueryServiceInterface
    {
        private readonly ImageQueryServiceInterface _imageQueryService;

        public CachedImageQueryService([SourceDataSource]ImageQueryServiceInterface imageQueryService
                , ObjectCache cacheProvider            
                , int defaultSecondsToCache = -1) 
            : base(cacheProvider, CachedImageContext.Region, imageQueryService, defaultSecondsToCache)
        {
            _imageQueryService = imageQueryService;
        }

//        public ImageInterface FindById(string id)
//        {
//            //for now don't cache processing status because will
//            //be processing images in a separate process most likely
//            //if we switch to a shared cache then we can remove this check
//            return RetrieveCachedData(
//                GetKeyFor(CachedImageContext.Image, id),
//                () => _imageQueryService.FindById(id)
//                , i => i.Status != ImageStatus.Processing);          
//        }


    }
}