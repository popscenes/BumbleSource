using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Website.Test.Common
{
    public static class RepoCoreUtil
    {
        public static HashSet<T> GetMockStore<T>()
        {
            HashSet<T> store = null;
            store = new HashSet<T>();

            return store;
        }

        public static bool CopyAndStore<EntityType, EntityInterfaceType, StoreInterfaceType>(this HashSet<StoreInterfaceType> store
            , EntityInterfaceType source, Action<EntityInterfaceType, EntityInterfaceType> copyFields)
            where EntityType : class, EntityInterfaceType, new()
            where EntityInterfaceType : EntityIdInterface, StoreInterfaceType
            where StoreInterfaceType : EntityIdInterface
        {
            store.RemoveWhere(e => e.Id == source.Id);
            var newb = source.CreateCopy<EntityType, EntityInterfaceType>(copyFields);
            store.Add(newb);
            return true;
        }

        public static Mock<RepoType> SetupRepo<RepoType, EntityType, EntityInterfaceType, StoreInterfaceType>(
            HashSet<StoreInterfaceType> store
            , MoqMockingKernel kernel
            , Action<EntityInterfaceType, EntityInterfaceType> copyFields)
            where RepoType : class, GenericRepositoryInterface where EntityType : class, EntityInterfaceType, new()
            where EntityInterfaceType : EntityIdInterface, StoreInterfaceType
            where StoreInterfaceType : EntityIdInterface
        {
            var repository = kernel.GetMock<RepoType>();
            kernel.Rebind<RepoType>()
                .ToConstant(repository.Object).InSingletonScope();

            var repositoryGeneric = kernel.GetMock<GenericRepositoryInterface>();
            kernel.Rebind<GenericRepositoryInterface>()
                .ToConstant(repositoryGeneric.Object).InSingletonScope();

            Action<EntityInterfaceType> storeAction = entity =>
                {
                    store.CopyAndStore<EntityType, EntityInterfaceType, StoreInterfaceType>(entity, copyFields);
                    //find any top level aggregate members and store them
                    var aggMembers = new HashSet<object>();
                    AggregateMemberEntityAttribute.GetAggregateEnities(aggMembers, entity, false);
                    var repoForAggs = kernel.Get<GenericRepositoryInterface>();
                    foreach (dynamic aggMember in aggMembers.Where(m => !ReferenceEquals(m, entity)))
                    {
                       repoForAggs.Store(aggMember);
                    }
                };

            repository.Setup(o => o.Store(It.IsAny<EntityType>()))
                .Callback<EntityType>(storeAction);
            repository.Setup(o => o.Store(It.IsAny<EntityInterfaceType>()))
                .Callback<EntityInterfaceType>(storeAction);

            repositoryGeneric.Setup(o => o.Store(It.IsAny<EntityType>()))
                .Callback<EntityType>(storeAction);
            repositoryGeneric.Setup(o => o.Store(It.IsAny<EntityInterfaceType>()))
                .Callback<EntityInterfaceType>(storeAction);


            Action<string, Action<EntityType>> updateAction = (id, act) =>
                {
                    var entity = store.OfType<EntityType>().SingleOrDefault(b => b.Id == id);
                    act(entity);
                    storeAction(entity);
                };

            repository.Setup(m => m.UpdateEntity(It.IsAny<string>(), It.IsAny<Action<EntityType>>()))
                .Callback<string, Action<EntityType>>(updateAction);

            repositoryGeneric.Setup(m => m.UpdateEntity(It.IsAny<string>(), It.IsAny<Action<EntityType>>()))
                .Callback<string, Action<EntityType>>(updateAction);

            repository.Setup(m => m.UpdateEntity(typeof(EntityType), It.IsAny<string>(), It.IsAny<Action<object>>()))
                .Callback<Type, string, Action<EntityType>>((type, id, act) => updateAction(id, act));

            repositoryGeneric.Setup(m => m.UpdateEntity(typeof(EntityType), It.IsAny<string>(), It.IsAny<Action<object>>()))
                .Callback<Type, string, Action<EntityType>>((type, id, act) => updateAction(id, act));

            repository.Setup(r => r.SaveChanges()).Returns(true);
            repositoryGeneric.Setup(r => r.SaveChanges()).Returns(true);

            return repository;
        }

        public static Mock<QsType> SetupQueryService<QsType, EntityType, EntityInterfaceType, StoreInterfaceType>(
            HashSet<StoreInterfaceType> store
            , MoqMockingKernel kernel
            , Action<EntityInterfaceType, EntityInterfaceType> copyFields)
            where QsType : class, GenericQueryServiceInterface where EntityType : class, EntityInterfaceType, new()
            where EntityInterfaceType : class, EntityIdInterface, StoreInterfaceType
            where StoreInterfaceType : EntityIdInterface
        {
            var queryService = kernel.GetMock<QsType>();
            var queryServiceGeneric = kernel.GetMock<GenericQueryServiceInterface>();

            kernel.Rebind<QsType>()
                .ToConstant(queryService.Object);
            kernel.Rebind<GenericQueryServiceInterface>()
                .ToConstant(queryServiceGeneric.Object);

            Func<string, EntityType> findById =
                id =>
                    {
                        //create a copy for find by id
                        var entity = new EntityType();
                        var stored = store.SingleOrDefault(f => f.Id == id);
                        if (stored == null) return null;
                        copyFields(entity, stored as EntityInterfaceType);
                        return entity;
                    };

            queryService.Setup(m => m.FindById<EntityType>(It.IsAny<string>()))
                .Returns<string>(findById);
            queryServiceGeneric.Setup(m => m.FindById<EntityType>(It.IsAny<string>()))
                .Returns<string>(findById);

            Func<Type, string, EntityType> findByTypeId = (type, id) => findById(id);

            queryService.Setup(m => m.FindById(typeof(EntityType), It.IsAny<string>()))
                .Returns<Type, string>(findByTypeId);
            queryServiceGeneric.Setup(m => m.FindById(typeof(EntityType), It.IsAny<string>()))
                .Returns<Type, string>(findByTypeId);



            Func<string, EntityType> findByFriendlyId =
            friendlyId =>
            {
                //create a copy for find by id
                var entity = new EntityType();
                var stored = store.SingleOrDefault(f => f.FriendlyId == friendlyId);
                if (stored == null) return null;
                copyFields(entity, stored as EntityInterfaceType);
                return entity;
            };

            queryService.Setup(m => m.FindByFriendlyId<EntityType>(It.IsAny<string>()))
                .Returns<string>(findByFriendlyId);
            queryServiceGeneric.Setup(m => m.FindByFriendlyId<EntityType>(It.IsAny<string>()))
                .Returns<string>(findByFriendlyId);

            Func<Type, string, EntityType> findFriendlyByTypeId = (type, id) => findById(id);
            queryService.Setup(m => m.FindByFriendlyId(typeof(EntityType), It.IsAny<string>()))
                .Returns<Type, string>(findFriendlyByTypeId);
            queryServiceGeneric.Setup(m => m.FindByFriendlyId(typeof(EntityType), It.IsAny<string>()))
                .Returns<Type, string>(findFriendlyByTypeId);

            return queryService;
        }

        public static Mock<QsType> FindAggregateEntities<QsType, EntityType, EntityInterfaceType>(
            HashSet<EntityInterfaceType> store
            , MoqMockingKernel kernel
            , Action<EntityInterfaceType, EntityInterfaceType> copyFields)
            where QsType : class, GenericQueryServiceInterface
            where EntityType : class, EntityInterfaceType,  new()
            where EntityInterfaceType : class, EntityInterface, AggregateInterface
        {
            var queryService = kernel.GetMock<QsType>();
            var queryServiceGeneric = kernel.GetMock<GenericQueryServiceInterface>();


            Func<string, IQueryable<string>> findById =
                (id) =>
                    {
                        var list = store.Where(f => f.AggregateId == id)
                            .Select(
                                e =>
                                    {
                                        var ret = new EntityType();
                                        copyFields(ret, e);
                                        return ret;
                                    }).AsQueryable();
                        return list.Select(e => e.Id);
                    };

            queryService.Setup(m => m.FindAggregateEntityIds<EntityType>(It.IsAny<string>()))
                .Returns<string>(findById);
            queryServiceGeneric.Setup(m => m.FindAggregateEntityIds<EntityType>(It.IsAny<string>()))
                .Returns<string>(findById);

            return queryService;
        }
    }


}
