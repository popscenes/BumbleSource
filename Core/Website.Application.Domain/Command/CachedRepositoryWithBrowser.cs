using System;
using System.Runtime.Caching;
using Website.Infrastructure.Caching.Command;
using Website.Infrastructure.Command;
using Website.Domain.Browser;

namespace Website.Application.Domain.Command
{
    public class CachedRepositoryWithBrowser : CachedRepositoryBase
    {
        public CachedRepositoryWithBrowser(ObjectCache cacheProvider
            , string regionName
            , GenericRepositoryInterface genericRepository) 
            : base(cacheProvider, regionName, genericRepository)
        {
        }

        public override void UpdateEntity<UpdateType>(string id, Action<UpdateType> updateAction)
        {
            Action<UpdateType> updateInvCacheAction
                = entity =>
                {             
                    updateAction(entity);
                    var brows = entity as BrowserIdInterface;
                    if(brows != null)
                        this.InvalidateCachedData(GetKeyFor("forbrowser", brows.BrowserId));
                };
            base.UpdateEntity(id, updateInvCacheAction);
        }

        public override void UpdateEntity(Type entity, string id, Action<object> updateAction)
        {
            Action<object> updateInvCacheAction
                = ent =>
                {
                    updateAction(ent);
                    var brows = ent as BrowserIdInterface;
                    if (brows != null)
                        this.InvalidateCachedData(GetKeyFor("forbrowser", brows.BrowserId));
                };
            base.UpdateEntity(entity, id, updateInvCacheAction);
        }

        public override void Store<EntityType>(EntityType entity)
        {
            var brows = entity as BrowserIdInterface;
            if (brows != null)
                this.InvalidateCachedData(GetKeyFor("forbrowser", brows.BrowserId));
            base.Store(entity);
        }
    }
}
