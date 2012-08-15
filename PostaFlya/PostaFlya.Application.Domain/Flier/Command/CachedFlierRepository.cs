using System;
using System.Runtime.Caching;
using WebSite.Application.Binding;
using WebSite.Application.Caching.Command;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Comments;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Likes;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Binding;

namespace PostaFlya.Application.Domain.Flier.Command
{
    internal class CachedFlierRepository : BroadcastCachedRepository,
                                            FlierRepositoryInterface
    {
        private readonly FlierRepositoryInterface _flierRepository;

        public CachedFlierRepository([SourceDataSource]FlierRepositoryInterface flierRepository
                , ObjectCache cacheProvider
                , CacheNotifier notifier)
            : base(cacheProvider, CachedFlierContext.Region, notifier)
        {
            _flierRepository = flierRepository;
        }

        public void Store(object entity)
        {
            var flier = entity as FlierInterface;
            if(flier != null)
                Store(flier);
        }

        public bool SaveChanges()
        {
            return _flierRepository.SaveChanges();
        }

        public void UpdateEntity(string id, Action<FlierInterface> updateAction)
        {
            Action<FlierInterface> updateInvCacheAction
                = flier =>
                      {
                          updateAction(flier);
                          this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Flier, flier.Id));
                          this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Browser, flier.BrowserId));
                      };
            _flierRepository.UpdateEntity(id, updateInvCacheAction);
        }

        public void Store(FlierInterface entity)
        {
            _flierRepository.Store(entity);
            this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Browser, entity.BrowserId));
        }

        public LikeableInterface Like(LikeInterface like)
        {
            var ret = _flierRepository.Like(like);
            if (ret != null)
            {
                var flierCreator = ret as BrowserIdInterface;
                if (flierCreator != null)
                    this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Browser, flierCreator.BrowserId));
                this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Likes, like.BrowserId));
                this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Likes, like.EntityId));
                this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Flier, like.EntityId));
            }
            return ret;
        }

        public CommentableInterface AddComment(CommentInterface comment)
        {
            var ret = _flierRepository.AddComment(comment);
            if (ret != null)
            {
                var flierCreator = ret as BrowserIdInterface;
                if(flierCreator != null)
                    this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Browser, flierCreator.BrowserId));
                this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Comments, comment.EntityId));
                this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Flier, comment.EntityId));
            }
                
            return ret;
        }
    }
}