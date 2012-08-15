using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.Domain.Likes;
using WebSite.Infrastructure.Query;

namespace PostaFlya.DataRepository.Internal
{
    internal class AzureLikeRepository :
        AzureRepositoryBase<LikeInterface, LikeStorageDomain>,
        GenericQueryServiceInterface<LikeInterface>
    {
        private readonly AzureTableContext _tableContext;
        public AzureLikeRepository([Named("likes")]AzureTableContext tableContext)
            : base(tableContext)
        {
            _tableContext = tableContext;
        }

        protected override LikeStorageDomain GetEntityForUpdate(string id)
        {
            return LikeStorageDomain.GetEntityForUpdate(id, _tableContext);
        }

        protected override LikeStorageDomain GetStorageForEntity(LikeInterface entity)
        {
            return new LikeStorageDomain(entity, _tableContext);
        }

        public LikeInterface FindById(string id)
        {
            return LikeStorageDomain.FindById(id, _tableContext);
        }

        public IQueryable<LikeInterface> FindByBrowserAndEntityTypeTag(string bropwserId, string entityTypeTag)
        {
            var ret = LikeStorageDomain.FindByBrowserAndEntityTypeTag(bropwserId, entityTypeTag, _tableContext);
            return ret.Distinct(new IsSameBrowserLike()).OrderByDescending(l => l.LikeTime);
        }

        object QueryServiceInterface.FindById(string id)
        {
            return FindById(id);
        }

        internal class IsSameBrowserLike : IEqualityComparer<LikeInterface>
        {
            public bool Equals(LikeInterface x, LikeInterface y)
            {
                return x.BrowserId == y.BrowserId && x.EntityId == y.EntityId;
            }

            public int GetHashCode(LikeInterface obj)
            {
                return obj.BrowserId.GetHashCode() ^ obj.EntityId.GetHashCode();
            }
        }
        public IQueryable<LikeInterface> GetByEntity(string entityId, int take = -1)
        {
            var ret = LikeStorageDomain.FindRelatedEntities(entityId, _tableContext, take);
            return ret.Distinct(new IsSameBrowserLike()).OrderBy(l => l.LikeTime);
        }
    }
}
