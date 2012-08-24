using System;
using System.Runtime.Caching;
using WebSite.Application.Binding;
using WebSite.Application.Caching.Command;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Binding;
using Website.Application.Domain.Command;

namespace PostaFlya.Application.Domain.Flier.Command
{
    internal class CachedFlierRepository : CachedRepositoryWithBrowser,
                                            FlierRepositoryInterface
    {

        public CachedFlierRepository([SourceDataSource]FlierRepositoryInterface flierRepository
                , ObjectCache cacheProvider)
            : base(cacheProvider, CachedFlierContext.Region, flierRepository)
        {
        }

//        public override void UpdateEntity<UpdateType>(string id, Action<UpdateType> updateAction)
//        {
//            Action<UpdateType> updateInvCacheAction
//                = flier =>
//                      {
//                          updateAction(flier);
//                          var target = flier as FlierInterface;
//                          this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Browser, target.BrowserId));
//                      };
//            base.UpdateEntity(id, updateInvCacheAction);
//        }
//
//        public override void Store<EntityType>(EntityType entity)
//        {
//            base.Store(entity);
//            var target = entity as FlierInterface;
//            this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Browser, target.BrowserId));
//        }

//        public LikeableInterface Like(LikeInterface like)
//        {
//            var ret = _flierRepository.Like(like);
//            if (ret != null)
//            {
//                var flierCreator = ret as BrowserIdInterface;
//                if (flierCreator != null)
//                    this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Browser, flierCreator.BrowserId));
//                this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Likes, like.BrowserId));
//                this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Likes, like.EntityId));
//                this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Flier, like.EntityId));
//            }
//            return ret;
//        }
//
//        public CommentableInterface AddComment(CommentInterface comment)
//        {
//            var ret = _flierRepository.AddComment(comment);
//            if (ret != null)
//            {
//                var flierCreator = ret as BrowserIdInterface;
//                if(flierCreator != null)
//                    this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Browser, flierCreator.BrowserId));
//                this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Comments, comment.EntityId));
//                this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Flier, comment.EntityId));
//            }
//                
//            return ret;
//        }
    }
}