using System;
using System.Runtime.Caching;
using Website.Application.Binding;
using Website.Application.Caching.Command;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Binding;
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

//        public ClaimableInterface Claim(ClaimInterface claim)
//        {
//            var ret = _flierRepository.Claim(claim);
//            if (ret != null)
//            {
//                var flierCreator = ret as BrowserIdInterface;
//                if (flierCreator != null)
//                    this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Browser, flierCreator.BrowserId));
//                this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Claims, claim.BrowserId));
//                this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Claims, claim.EntityId));
//                this.InvalidateCachedData(GetKeyFor(CachedFlierContext.Flier, claim.EntityId));
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