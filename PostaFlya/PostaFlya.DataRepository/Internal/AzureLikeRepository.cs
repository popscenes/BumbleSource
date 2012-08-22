//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Ninject;
//using WebSite.Azure.Common.TableStorage;
//using PostaFlya.Domain.Likes;
//using WebSite.Infrastructure.Query;
//
//namespace PostaFlya.DataRepository.Internal
//{
//    internal class AzureLikeRepository : JsonRepository
//    {
//        public const int BrowserPartition = 1;
//        public AzureLikeRepository(TableContextInterface tableContext
//            , TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService) 
//            : base(tableContext, nameAndPartitionProviderService)
//        {
//        }
//
//        public static string GetIdPartitionKey(LikeInterface like)
//        {
//            return like.AggregateId + like.BrowserId;
//        }
//
//        public IQueryable<LikeInterface> FindByBrowserAndEntityTypeTag(string bropwserId, string entityTypeTag)
//        {
//            var ret = FindEntitiesByPartition<Like>(bropwserId, BrowserPartition);
//            return ret.Distinct(new IsSameBrowserLike())
//                .Where(l => l.EntityTypeTag == entityTypeTag)
//                .OrderByDescending(l => l.LikeTime);
//        }
//
//        internal class IsSameBrowserLike : IEqualityComparer<LikeInterface>
//        {
//            public bool Equals(LikeInterface x, LikeInterface y)
//            {
//                return x.BrowserId == y.BrowserId && x.AggregateId == y.AggregateId;
//            }
//
//            public int GetHashCode(LikeInterface obj)
//            {
//                return obj.BrowserId.GetHashCode() ^ obj.AggregateId.GetHashCode();
//            }
//        }
//        public IQueryable<LikeInterface> GetByEntity(string entityId, int take = -1)
//        {
//            var ret = FindAggregateEntities<Like>(entityId, take);
//            return ret.Distinct(new IsSameBrowserLike()).OrderBy(l => l.LikeTime);
//        }
//    }
//}
