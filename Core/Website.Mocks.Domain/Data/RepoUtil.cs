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

namespace Website.Mocks.Domain.Data
{
    public static class RepoUtil
    {
        public static HashSet<T> GetMockStore<T>()
        {
            HashSet<T> store = null;
            store = new HashSet<T>();

            return store;
        }

        public static bool CopyAndStore<EntityType, EntityInterfaceType>(this HashSet<EntityInterfaceType> store
            , EntityInterfaceType source, Action<EntityInterfaceType, EntityInterfaceType> copyFields)
            where EntityType : class, EntityInterfaceType, new() 
            where EntityInterfaceType : EntityIdInterface
        {
            store.RemoveWhere(e => e.Id == source.Id);
            var newb = source.CreateCopy<EntityType, EntityInterfaceType>(copyFields);
            store.Add(newb);
            return true;
        }

        public static Mock<RepoType> SetupRepo<RepoType, EntityType, EntityInterfaceType>(
            HashSet<EntityInterfaceType> store
            , MoqMockingKernel kernel
            , Action<EntityInterfaceType, EntityInterfaceType> copyFields)
            where RepoType : class, GenericRepositoryInterface where EntityType : class, EntityInterfaceType, new()
            where EntityInterfaceType : EntityIdInterface
        {
            var repository = kernel.GetMock<RepoType>();
            kernel.Rebind<RepoType>()
                .ToConstant(repository.Object).InSingletonScope();

            var repositoryGeneric = kernel.GetMock<GenericRepositoryInterface>();
            kernel.Rebind<GenericRepositoryInterface>()
                .ToConstant(repositoryGeneric.Object).InSingletonScope();

            Action<EntityInterfaceType> storeAction = entity =>
                {
                    store.CopyAndStore<EntityType, EntityInterfaceType>(entity, copyFields);
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

        public static Mock<QsType> SetupQueryService<QsType, EntityType, EntityInterfaceType>(
            HashSet<EntityInterfaceType> store
            , MoqMockingKernel kernel
            , Action<EntityInterfaceType, EntityInterfaceType> copyFields)
            where QsType : class, GenericQueryServiceInterface where EntityType : class, EntityInterfaceType, new()
            where EntityInterfaceType : class, EntityIdInterface
        {
            var queryService = kernel.GetMock<QsType>();
            var queryServiceGeneric = kernel.GetMock<GenericQueryServiceInterface>();
            var queryServiceWithBrowser = (typeof (QueryServiceForBrowserAggregateInterface)
                                              .IsAssignableFrom(typeof (QsType)))
                                              ? kernel.GetMock<QueryServiceForBrowserAggregateInterface>()
                                              : null;

            kernel.Rebind<QsType>()
                .ToConstant(queryService.Object);
            kernel.Rebind<GenericQueryServiceInterface>()
                .ToConstant(queryServiceGeneric.Object);
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

            queryService.Setup(m => m.FindById<EntityType>(It.IsAny<string>()))
                .Returns<string>(findById);
            queryServiceGeneric.Setup(m => m.FindById<EntityType>(It.IsAny<string>()))
                .Returns<string>(findById);
            if(queryServiceWithBrowser != null)
                queryServiceWithBrowser.Setup(m => m.FindById<EntityType>(It.IsAny<string>()))
                    .Returns<string>(findById);

            Func<Type, string, EntityType> findByTypeId = (type, id) => findById(id);

            queryService.Setup(m => m.FindById(typeof(EntityType), It.IsAny<string>()))
                .Returns<Type, string>(findByTypeId);
            queryServiceGeneric.Setup(m => m.FindById(typeof(EntityType), It.IsAny<string>()))
                .Returns<Type, string>(findByTypeId);
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

            queryService.Setup(m => m.FindByFriendlyId<EntityType>(It.IsAny<string>()))
                .Returns<string>(findByFriendlyId);
            queryServiceGeneric.Setup(m => m.FindByFriendlyId<EntityType>(It.IsAny<string>()))
                .Returns<string>(findByFriendlyId);
            if(queryServiceWithBrowser != null)
                queryServiceWithBrowser.Setup(m => m.FindByFriendlyId<EntityType>(It.IsAny<string>()))
                .Returns<string>(findByFriendlyId);

            Func<Type, string, EntityType> findFriendlyByTypeId = (type, id) => findById(id);
            queryService.Setup(m => m.FindByFriendlyId(typeof(EntityType), It.IsAny<string>()))
                .Returns<Type, string>(findFriendlyByTypeId);
            queryServiceGeneric.Setup(m => m.FindByFriendlyId(typeof(EntityType), It.IsAny<string>()))
                .Returns<Type, string>(findFriendlyByTypeId);
            if (queryServiceWithBrowser != null)
                queryServiceWithBrowser.Setup(m => m.FindByFriendlyId(typeof(EntityType), It.IsAny<string>()))
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
            //            var queryService = kernel.MockRepository.Create<QsType>();
            //            var queryServiceBase = queryService.As<QueryServiceInterface>();

//            kernel.Bind<QsType>()
//                .ToConstant(queryService.Object);
//            kernel.Rebind<GenericQueryServiceInterface>()
//                .ToConstant(queryServiceGeneric.Object);

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


//        public static Mock<QsType> SetupQueryComments<QsType, EntityInterfaceType>(
//            Mock<QsType> queryService,
//            HashSet<CommentInterface> store
//            , MoqMockingKernel kernel)
//            where QsType : class, QueryCommentsInterface<EntityInterfaceType> where EntityInterfaceType : class, BrowserIdInterface, CommentableInterface
//        {
//            kernel.Bind<QueryCommentsInterface<EntityInterfaceType>>()
//                .ToConstant(queryService.Object);
//
//            queryService.Setup(o => o.GetComments(It.IsAny<string>(), It.IsAny<int>()))
//            .Returns<String, int>((id, take) =>
//                store.Where(c => c.EntityId == id)
//                .Select(c =>
//                {
//                    var com = new Comment()
//                                  {
//                                      BrowserId = c.BrowserId,
//                                      EntityId = c.EntityId,
//                                      CommentContent = c.CommentContent
//                                  };
//                    return com;
//                }).Take( take == -1 ? store.Count : take)
//                .AsQueryable());
//            return queryService;
//        }


//        internal static Mock<RepoType> SetupAddComment<RepoType, EntityInterfaceType, EntityType>(
//            Mock<RepoType> repository,
//            HashSet<CommentInterface> store
//            , MoqMockingKernel kernel)
//            where RepoType : class, AddCommentInterface<EntityInterfaceType>, GenericRepositoryInterface
//            where EntityInterfaceType : class, CommentableInterface
//            where EntityType : class, EntityInterfaceType, new()
//        {
//            kernel.Bind<AddCommentInterface<EntityInterfaceType>>()
//                .ToConstant(repository.Object);
//
//            repository.Setup(o => o.AddComment(It.IsAny<CommentInterface>()))
//                .Returns<CommentInterface>(c =>
//                {
//                    CommentableInterface entity = null;
//                    repository.Object.UpdateEntity<EntityType>(c.EntityId, e =>
//                    {
//                        entity = e;
//                        if(entity != null)
//                            e.NumberOfComments++;
//                    });
//                    var com = new Comment();
//                    com.CopyFieldsFrom(c);
//                    com.Id = com.Id ?? Guid.NewGuid().ToString();
//                    store.Add(com);
//                    return store.Any(comment => string.IsNullOrWhiteSpace(comment.Id)) ? null : entity;
//                });
//
//            return repository;
//        }


//        internal static Mock<RepoType> SetupAddLike<RepoType, EntityInterfaceType, EntityType>(
//            Mock<RepoType> repository,
//            IList<LikeInterface> store
//            , MoqMockingKernel kernel)
//            where RepoType : class, AddLikeInterface<EntityInterfaceType>, GenericRepositoryInterface 
//            where EntityInterfaceType : class, LikeableInterface
//            where EntityType : class, EntityInterfaceType, new()
//        {
//            kernel.Bind<AddLikeInterface<EntityInterfaceType>>()
//                .ToConstant(repository.Object);
//
//            repository.Setup(o => o.Like(It.IsAny<LikeInterface>()))
//                .Returns<LikeInterface>((l) =>
//                {
//                    
//                    var like = store.SingleOrDefault(ls =>
//                        ls.BrowserId.Equals(l.BrowserId)
//                        && ls.EntityId == l.EntityId);
//
//                    LikeableInterface entity = null;
//                    if (like != null)
//                        store.Remove(like);
//
//                    repository.Object.UpdateEntity<EntityType>(l.EntityId
//                    , e =>
//                            {
//                                entity = e;
//                                if(like == null)
//                                    e.NumberOfLikes++;
//                            });
//                    
//
//                    var newl = new Like()
//                    {
//                        AggregateTypeTag = typeof(EntityInterfaceType).Name,
//                        BrowserId = l.BrowserId,
//                        LikeContent = l.LikeContent,
//                        EntityId = l.EntityId,
//                        ILike = l.ILike,
//                    };
//                    store.Add(newl);
//
//                    return entity;
//                });
//
//            return repository;
//        }

//        public static Mock<QsType> SetupQueryLike<QsType, EntityInterfaceType>(
//            Mock<QsType> queryService,
//            IList<LikeInterface> store
//            , MoqMockingKernel kernel)
//            where QsType : class, QueryLikesInterface<EntityInterfaceType>, GenericQueryServiceInterface
//            where EntityInterfaceType : class, LikeableInterface
//        {
//            kernel.Bind<QueryLikesInterface<EntityInterfaceType>>()
//                .ToConstant(queryService.Object);
//
//            queryService.Setup(o => o.GetLikes(It.IsAny<string>()))
//            .Returns<String>(id =>
//                store.Where(like => like.EntityId == id)
//                .Select(like =>
//                {
//                    var lik = new Like();
//                    lik.CopyFieldsFrom(like);
//                    return lik;
//                })
//                .AsQueryable());
//
//            queryService.Setup(o => o.GetLikesByBrowser(It.IsAny<string>()))
//            .Returns<String>(id =>
//                store.Where(like => like.BrowserId == id && like.AggregateTypeTag == typeof(EntityInterfaceType).Name)
//                .AsQueryable());
//            return queryService;
//        }
    }


}
