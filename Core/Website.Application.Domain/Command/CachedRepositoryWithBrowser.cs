using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using Website.Application.Domain.Query;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Caching.Command;
using Website.Infrastructure.Caching.Query;
using Website.Infrastructure.Command;
using Website.Domain.Browser;
using Website.Infrastructure.Domain;

namespace Website.Application.Domain.Command
{
    public class CachedRepositoryWithBrowser : CachedRepositoryBase
    {
        public CachedRepositoryWithBrowser(ObjectCache cacheProvider
            , [SourceDataSource] GenericRepositoryInterface genericRepository) 
            : base(cacheProvider, genericRepository)
        {
        }

        protected override void InvalidateEntity(object entity)
        {
            base.InvalidateEntity(entity);
            var entitiesIn = new HashSet<object>();
            AggregateMemberEntityAttribute.GetAggregateEnities(entitiesIn, entity);
            foreach (var ent in entitiesIn.OfType<BrowserIdInterface>())
            {
                this.InvalidateCachedData(ent.BrowserId.GetCacheKeyFor(ent.GetType(), "BrowserId"));
            }
        }
    }
}
