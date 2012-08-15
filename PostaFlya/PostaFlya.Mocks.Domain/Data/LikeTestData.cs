using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MbUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Domain.Likes;
using PostaFlya.Domain.Likes.Command;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Query;

namespace PostaFlya.Mocks.Domain.Data
{
    public static class LikeTestData
    {
        public static bool AssertStoreRetrieve(LikeInterface storedLike, LikeInterface retrievedLike)
        {
            Assert.AreEqual(storedLike.EntityId, retrievedLike.EntityId);
            Assert.AreEqual(storedLike.BrowserId, retrievedLike.BrowserId);
            Assert.AreEqual(storedLike.LikeContent, retrievedLike.LikeContent);
            Assert.AreApproximatelyEqual(storedLike.LikeTime, retrievedLike.LikeTime, TimeSpan.FromMilliseconds(1));

            return true;
        }

        public static bool Equals(LikeInterface storedLike, LikeInterface retrievedLike)
        {
            return storedLike.ILike == retrievedLike.ILike &&
                   storedLike.EntityId == retrievedLike.EntityId &&
                   storedLike.LikeContent == retrievedLike.LikeContent && 
                   storedLike.BrowserId == retrievedLike.BrowserId &&
                   storedLike.LikeTime - retrievedLike.LikeTime < TimeSpan.FromMilliseconds(1);
        }

        internal static LikeInterface AssertGetById(LikeInterface like, GenericQueryServiceInterface<LikeInterface> queryService)
        {
            var retrievedFlier = queryService.FindById(like.EntityId + like.BrowserId);
            AssertStoreRetrieve(like, retrievedFlier);

            return retrievedFlier;
        }

        internal static LikeInterface LikeOne<EntityInterfaceType>(LikeInterface like, AddLikeInterface<EntityInterfaceType> repository, StandardKernel kernel) where EntityInterfaceType : LikeableInterface
        {
            using (kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<object>() { repository }))
            {

                repository.Like(like);
            }
            return like;
        }

        internal static LikeInterface StoreOne(LikeInterface like, GenericRepositoryInterface<LikeInterface> repository, StandardKernel kernel)
        {
            using (kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository }))
            {

                repository.Store(like);
            }
            return like;
        }

        internal static IList<LikeInterface> StoreSome(GenericRepositoryInterface<LikeInterface> repository, StandardKernel kernel, string entityId, int num = 5)
        {

            var ret = GetSome(kernel, entityId, num);
            foreach (var like in ret)
            {
                UnitOfWorkInterface unitOfWork =
                    kernel.Get<UnitOfWorkFactoryInterface>().GetUnitOfWork(new List<RepositoryInterface>() {repository});
                using (unitOfWork)
                {

                    repository.Store(like);
                }
                Assert.IsTrue(unitOfWork.Successful);
            }
            return ret;
        }

        internal static IList<LikeInterface> GetSome(StandardKernel kernel, string entityId, int num  = 100)
        {
            var time = DateTime.UtcNow;
            var ret = new List<LikeInterface>();
            for (int i = 0; i < num; i++)
            {
                var like = GetOne(kernel, entityId);
                like.LikeContent = "Like number " + i;
                like.LikeTime = time;
                like.EntityTypeTag = "TestEntityType";
                ret.Add(like);
                time = time.AddMinutes(10);
            }
            return ret;
        }

        internal static void UpdateOne(LikeInterface like, GenericRepositoryInterface<LikeInterface> repository, StandardKernel kernel)
        {
            using (kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository }))
            {
                repository.UpdateEntity(like.EntityId + like.BrowserId, e => e.CopyFieldsFrom(like));
            }
        }

        public static LikeInterface GetOne(StandardKernel kernel, string entityId)
        {   
            var ret = new Like
                          {
                              EntityId = entityId,
                              BrowserId = Guid.NewGuid().ToString(),
                              LikeContent = "This is a like",
                              EntityTypeTag = "TestEntityType",
                              ILike = true,
                              LikeTime = DateTime.UtcNow
                          };
            return ret;
        }
    }
}
