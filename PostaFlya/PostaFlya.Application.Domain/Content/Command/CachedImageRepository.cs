using System;
using System.Runtime.Caching;
using WebSite.Application.Binding;
using WebSite.Application.Caching.Command;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Content.Command;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Binding;

namespace PostaFlya.Application.Domain.Content.Command
{
    internal class CachedImageRepository : BroadcastCachedRepository,
                                           ImageRepositoryInterface
    {
        private readonly ImageRepositoryInterface _imageRepository;

        public CachedImageRepository([SourceDataSource]ImageRepositoryInterface imageRepository
                , ObjectCache cacheProvider
                , CacheNotifier notifier)
            : base(cacheProvider, CachedImageContext.Region, notifier)
        {
            _imageRepository = imageRepository;
        }

        public void Store(object entity)
        {
            var image = entity as ImageInterface;
            if(image != null)
                Store(image);
        }

        public bool SaveChanges()
        {
            return _imageRepository.SaveChanges();
        }

        public void UpdateEntity(string id, Action<ImageInterface> updateAction)
        {
            Action<ImageInterface> updateInvCacheAction
                = image =>
                    {
                        updateAction(image);
                        InvalidateCachedData(GetKeyFor(CachedImageContext.Image, image.Id));
                        InvalidateCachedData(GetKeyFor(CachedImageContext.Browser, image.BrowserId));
                    };

            _imageRepository.UpdateEntity(id, updateInvCacheAction);
        }

        public void Store(ImageInterface entity)
        {
            _imageRepository.Store(entity);
            InvalidateCachedData(GetKeyFor(CachedImageContext.Browser, entity.BrowserId));
        }
    }
}