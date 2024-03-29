using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Ninject;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Publish;
using Website.Infrastructure.Query;
using Website.Domain.Claims;
using Website.Test.Common;

namespace Website.Mocks.Domain.Data
{
    public static class ClaimTestData
    {
        public static bool AssertStoreRetrieve(ClaimInterface storedClaim, ClaimInterface retrievedClaim)
        {
            Assert.AreEqual(storedClaim.AggregateId, retrievedClaim.AggregateId);
            Assert.AreEqual(storedClaim.BrowserId, retrievedClaim.BrowserId);
            Assert.AreEqual(storedClaim.ClaimContext, retrievedClaim.ClaimContext);
            AssertUtil.AreEqual(storedClaim.ClaimTime, retrievedClaim.ClaimTime, TimeSpan.FromMilliseconds(1));

            return true;
        }

        public class ClaimTestDataEq : IComparer
        {
            public int Compare(object x, object y)
            {
                return ClaimTestData.Equals((ClaimInterface)x, (ClaimInterface)y) ? 0 : 1;
            }
        }

        public static bool Equals(ClaimInterface storedClaim, ClaimInterface retrievedClaim)
        {
            return 
                   storedClaim.AggregateId == retrievedClaim.AggregateId &&
                   storedClaim.ClaimContext == retrievedClaim.ClaimContext && 
                   storedClaim.BrowserId == retrievedClaim.BrowserId &&
                   storedClaim.ClaimTime - retrievedClaim.ClaimTime < TimeSpan.FromMilliseconds(1);
        }

        internal static ClaimInterface AssertGetById(ClaimInterface claim, GenericQueryServiceInterface queryService)
        {
            var retrievedFlier = queryService.FindByAggregate<Claim>(claim.AggregateId + claim.BrowserId, claim.AggregateId);
            AssertStoreRetrieve(claim, retrievedFlier);

            return retrievedFlier;
        }

        internal static ClaimInterface ClaimOne<EntityInterfaceType>(EntityInterfaceType entity
            , ClaimInterface claim
            , GenericRepositoryInterface repository
            , StandardKernel kernel) 
            where EntityInterfaceType : EntityInterface, ClaimableEntityInterface
        {
            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = kernel.Get<UnitOfWorkFactoryInterface>().GetUnitOfWork(new List<object>() {repository}))
            {
                claim.Id = claim.GetId();
                repository.Store(claim);
                repository.UpdateEntity(((Object) entity).GetType()
                    , entity.Id
                    , o =>
                          {
                              var targ = o as ClaimableEntityInterface;
                              targ.NumberOfClaims++;
                          } );

            }


//            if (unitOfWork.Successful)
//            {
//                var indexers = kernel.GetAll<HandleEventInterface<ClaimEvent>>();
//                foreach (var handleEvent in indexers)
//                {
//                    handleEvent.Handle(new ClaimEvent() { Entity = (Claim)claim });
//                }
//            }
            return claim;
        }

        internal static ClaimInterface StoreOne(ClaimInterface claim, GenericRepositoryInterface repository, StandardKernel kernel)
        {
            var uow = kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() {repository});
            using (uow)
            {

                repository.Store(claim);
            }

            Assert.IsTrue(uow.Successful);

//            if (uow.Successful)
//            {
//                var indexers = kernel.GetAll<HandleEventInterface<ClaimEvent>>();
//                foreach (var handleEvent in indexers)
//                {
//                    handleEvent.Handle(new ClaimEvent() { Entity = (Claim)claim });
//                }
//            }

            return claim;

        }

        internal static IList<ClaimInterface> StoreSome(GenericRepositoryInterface repository, StandardKernel kernel, string entityId, int num = 5)
        {

            var ret = GetSome(kernel, entityId, num);
            foreach (var claim in ret)
            {
                UnitOfWorkInterface unitOfWork =
                    kernel.Get<UnitOfWorkFactoryInterface>().GetUnitOfWork(new List<RepositoryInterface>() {repository});
                using (unitOfWork)
                {

                    repository.Store(claim);
                }
                Assert.IsTrue(unitOfWork.Successful);
//                if (unitOfWork.Successful)
//                {
//                    var indexers = kernel.GetAll<HandleEventInterface<ClaimEvent>>();
//                    foreach (var handleEvent in indexers)
//                    {
//                        handleEvent.Handle(new ClaimEvent() { Entity = (Claim)claim });
//                    }
//                }
            }
            return ret;
        }

        internal static IList<ClaimInterface> GetSome(StandardKernel kernel, string entityId, int num  = 100)
        {
            var time = DateTime.UtcNow;
            var ret = new List<ClaimInterface>();
            for (int i = 0; i < num; i++)
            {
                var claim = GetOne(kernel, entityId);
                claim.Id = claim.GetId();
                claim.ClaimContext = "Claim number " + i;
                claim.ClaimTime = time;
                claim.AggregateTypeTag = "TestEntityType";
                ret.Add(claim);
                time = time.AddMinutes(10);
            }
            return ret;
        }

        internal static void UpdateOne(ClaimInterface claim, GenericRepositoryInterface repository, StandardKernel kernel)
        {
            Claim oldState = null;
            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = kernel.Get<UnitOfWorkFactoryInterface>().GetUnitOfWork(new List<RepositoryInterface>() {repository}))
            {
                repository.UpdateAggregateEntity<Claim>(claim.AggregateId + claim.BrowserId
                    , claim.AggregateId
                    , e =>
                        {
                            oldState = e.CreateCopy<Claim, Claim>(ClaimInterfaceExtensions.CopyFieldsFrom);
                            e.CopyFieldsFrom(claim);
                        });
            }

//            if (unitOfWork.Successful)
//            {
//                var indexers = kernel.GetAll<HandleEventInterface<ClaimEvent>>();
//                foreach (var handleEvent in indexers)
//                {
//                    handleEvent.Handle(new ClaimEvent() { Entity = (Claim)claim});
//                }
//            }
        }

        public static ClaimInterface GetOne(StandardKernel kernel, string entityId)
        {   
            var ret = new Claim
                          {
                              AggregateId = entityId,
                              BrowserId = Guid.NewGuid().ToString(),
                              ClaimContext = "This is a claim",
                              AggregateTypeTag = "TestEntityType",
                              ClaimTime = DateTime.UtcNow
                          };
            ret.Id = ret.GetId();
            return ret;
        }
    }
}
