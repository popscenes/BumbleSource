using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;
using Website.Test.Common;

namespace Website.Mocks.Domain.Data
{
    public static class RepoUtil
    {
//        public static HashSet<T> GetMockStore<T>()
//        {
//            HashSet<T> store = null;
//            store = new HashSet<T>();
//
//            return store;
//        }
//
//        public static bool CopyAndStore<EntityType, EntityInterfaceType>(this HashSet<EntityInterfaceType> store
//            , EntityInterfaceType source, Action<EntityInterfaceType, EntityInterfaceType> copyFields)
//            where EntityType : class, EntityInterfaceType, new() 
//            where EntityInterfaceType : EntityIdInterface
//        {
//            store.RemoveWhere(e => e.Id == source.Id);
//            var newb = source.CreateCopy<EntityType, EntityInterfaceType>(copyFields);
//            store.Add(newb);
//            return true;
//        }

        public static Mock<RepoType> SetupRepo<RepoType, EntityType, EntityInterfaceType>(
            HashSet<EntityInterfaceType> store
            , MoqMockingKernel kernel
            , Action<EntityInterfaceType, EntityInterfaceType> copyFields)
            where RepoType : class, GenericRepositoryInterface where EntityType : class, EntityInterfaceType, new()
            where EntityInterfaceType : EntityIdInterface
        {
            return RepoCoreUtil.SetupRepo<RepoType, EntityType, EntityInterfaceType, EntityInterfaceType>(store, kernel, copyFields);
        }

        public static Mock<QsType> SetupQueryService<QsType, EntityType, EntityInterfaceType, StoreType>(
            HashSet<StoreType> store
            , MoqMockingKernel kernel
            , Action<EntityInterfaceType, EntityInterfaceType> copyFields)
            where QsType : class, GenericQueryServiceInterface where EntityType : class, EntityInterfaceType, new()
            where EntityInterfaceType : class, EntityIdInterface
            where StoreType : EntityInterfaceType
        {

            var ret = RepoCoreUtil.SetupQueryService<QsType, EntityType, EntityInterfaceType, StoreType>(store, kernel, copyFields);
            var queryServiceWithBrowser = (typeof (QueryServiceForBrowserAggregateInterface)
                                              .IsAssignableFrom(typeof (QsType)))
                                              ? kernel.GetMock<QueryServiceForBrowserAggregateInterface>()
                                              : null;

            if(queryServiceWithBrowser != null)
                kernel.Rebind<QueryServiceForBrowserAggregateInterface>()
                .ToConstant(queryServiceWithBrowser.Object);

            Func<string, EntityType> findById =
                id =>
                    {
                        //create a copy for find by id
                        var entity = new EntityType();
                        var stored = store.SingleOrDefault(f => f.Id == id);
                        if (stored == null) return null;
                        copyFields(entity, stored);
                        return entity;
                    };

            if(queryServiceWithBrowser != null)
                queryServiceWithBrowser.Setup(m => m.FindById<EntityType>(It.IsAny<string>()))
                    .Returns<string>(findById);

            Func<Type, string, EntityType> findByTypeId = (type, id) => findById(id);

            if(queryServiceWithBrowser != null)
                queryServiceWithBrowser.Setup(m => m.FindById(typeof(EntityType), It.IsAny<string>()))
                .Returns<Type, string>(findByTypeId);


            Func<string, EntityType> findByFriendlyId =
            friendlyId =>
            {
                //create a copy for find by id
                var entity = new EntityType();
                var stored = store.SingleOrDefault(f => f.FriendlyId == friendlyId);
                if (stored == null) return null;
                copyFields(entity, stored);
                return entity;
            };

            if(queryServiceWithBrowser != null)
                queryServiceWithBrowser.Setup(m => m.FindByFriendlyId<EntityType>(It.IsAny<string>()))
                .Returns<string>(findByFriendlyId);

            Func<Type, string, EntityType> findFriendlyByTypeId = (type, id) => findById(id);

            if (queryServiceWithBrowser != null)
                queryServiceWithBrowser.Setup(m => m.FindByFriendlyId(typeof(EntityType), It.IsAny<string>()))
                .Returns<Type, string>(findFriendlyByTypeId);

            return ret;
        }

        public static Mock<QsType> FindAggregateEntities<QsType, EntityType, EntityInterfaceType>(
            HashSet<EntityInterfaceType> store
            , MoqMockingKernel kernel
            , Action<EntityInterfaceType, EntityInterfaceType> copyFields)
            where QsType : class, GenericQueryServiceInterface
            where EntityType : class, EntityInterfaceType,  new()
            where EntityInterfaceType : class, EntityInterface, AggregateInterface
        {
            return RepoCoreUtil.FindAggregateEntities<QsType, EntityType, EntityInterfaceType>(store, kernel, copyFields);
        }

        public static Mock<QsType> SetupQueryByBrowser<QsType, EntityType, EntityInterfaceType>(
            Mock<QsType> queryService,
            HashSet<EntityInterfaceType> store
            , MoqMockingKernel kernel
            , Action<EntityInterfaceType, EntityInterfaceType> copyFields)
            where QsType : class, QueryServiceForBrowserAggregateInterface where EntityType : class, EntityInterfaceType, new()
            where EntityInterfaceType : class, BrowserIdInterface, EntityInterface
        {
//            kernel.Bind<QueryByBrowserInterface>()
//                .ToConstant(queryService.Object);

            var queryServiceGeneric = kernel.GetMock<QueryServiceForBrowserAggregateInterface>();
            kernel.Rebind<QueryServiceForBrowserAggregateInterface>()
                .ToConstant(queryServiceGeneric.Object);

            Func<string, IQueryable<string>> findByBrows = id =>
                                                           store.Where(f => f.BrowserId == id)
                                                                .Select(f =>
                                                                    {
                                                                        var fli = new EntityType();
                                                                        copyFields(fli, f);
                                                                        return fli;
                                                                    }).Select(e => e.Id)
                                                                .AsQueryable();

            queryService.Setup(o => o.GetEntityIdsByBrowserId<EntityType>(It.IsAny<string>()))
                .Returns<String>(findByBrows);
            queryServiceGeneric.Setup(o => o.GetEntityIdsByBrowserId<EntityType>(It.IsAny<string>()))
                .Returns<String>(findByBrows);
            
            return queryService;
        }

    }


}
