using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Ninject.MockingKernel.Moq;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Query;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Query;

namespace PostaFlya.Mocks.Domain.Data
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
            where EntityInterfaceType : EntityInterface
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
            where EntityInterfaceType : EntityInterface
        {
            var repository = kernel.GetMock<RepoType>();
            kernel.Rebind<RepoType>()
                .ToConstant(repository.Object).InSingletonScope();

            var repositoryGeneric = kernel.GetMock<GenericRepositoryInterface>();
            kernel.Rebind<GenericRepositoryInterface>()
                .ToConstant(repositoryGeneric.Object).InSingletonScope();

            repository.Setup(o => o.Store(It.IsAny<EntityType>()))
                .Callback<EntityType>(entity => 
                    store.CopyAndStore<EntityType, EntityInterfaceType>(entity, copyFields));
            repository.Setup(o => o.Store(It.IsAny<EntityInterfaceType>()))
                .Callback<EntityInterfaceType>(entity =>
                    store.CopyAndStore<EntityType, EntityInterfaceType>(entity, copyFields));

            repositoryGeneric.Setup(o => o.Store(It.IsAny<EntityType>()))
                .Callback<EntityType>(entity => 
                    store.CopyAndStore<EntityType, EntityInterfaceType>(entity, copyFields));
            repositoryGeneric.Setup(o => o.Store(It.IsAny<EntityInterfaceType>()))
                .Callback<EntityInterfaceType>(entity =>
                    store.CopyAndStore<EntityType, EntityInterfaceType>(entity, copyFields));

            repository.Setup(m => m.UpdateEntity(It.IsAny<string>(), It.IsAny<Action<EntityType>>()))
                .Callback<string, Action<EntityType>>((id, act) =>
                                 act(store.OfType<EntityType>().SingleOrDefault(b => b.Id == id)));

            repositoryGeneric.Setup(m => m.UpdateEntity(It.IsAny<string>(), It.IsAny<Action<EntityType>>()))
                .Callback<string, Action<EntityType>>((id, act) =>
                                 act(store.OfType<EntityType>().SingleOrDefault(b => b.Id == id)));

            repository.Setup(m => m.UpdateEntity(typeof(EntityType), It.IsAny<string>(), It.IsAny<Action<object>>()))
                .Callback<Type, string, Action<EntityType>>((type, id, act) =>
                     act(store.OfType<EntityType>().SingleOrDefault(b => b.Id == id)));

            repositoryGeneric.Setup(m => m.UpdateEntity(typeof(EntityType), It.IsAny<string>(), It.IsAny<Action<object>>()))
                .Callback<Type, string, Action<EntityType>>((type, id, act) =>
                     act(store.OfType<EntityType>().SingleOrDefault(b => b.Id == id)));

            repository.Setup(r => r.SaveChanges()).Returns(true);
            repositoryGeneric.Setup(r => r.SaveChanges()).Returns(true);

            return repository;
        }

        public static Mock<QsType> SetupQueryService<QsType, EntityType, EntityInterfaceType>(
            HashSet<EntityInterfaceType> store
            , MoqMockingKernel kernel
            , Action<EntityInterfaceType, EntityInterfaceType> copyFields)
            where QsType : class, GenericQueryServiceInterface where EntityType : class, EntityInterfaceType, new()
            where EntityInterfaceType : class, EntityInterface
        {
            var queryService = kernel.GetMock<QsType>();
            var queryServiceGeneric = kernel.GetMock<GenericQueryServiceInterface>();
//            var queryService = kernel.MockRepository.Create<QsType>();
//            var queryServiceBase = queryService.As<QueryServiceInterface>();

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
                        copyFields(entity, stored);
                        return entity;
                    };

            Func<Type, string, EntityType> findByTypeId = (type, id) => findById(id);

            queryService.Setup(m => m.FindById<EntityType>(It.IsAny<string>()))
                .Returns<string>(findById);
            queryServiceGeneric.Setup(m => m.FindById<EntityType>(It.IsAny<string>()))
                .Returns<string>(findById);

            queryService.Setup(m => m.FindById(typeof(EntityType), It.IsAny<string>()))
                .Returns<Type, string>(findByTypeId);
            queryServiceGeneric.Setup(m => m.FindById(typeof(EntityType), It.IsAny<string>()))
                .Returns<Type, string>(findByTypeId);

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

            Func<string, int, IQueryable<EntityType>> findById =
                (id, take) =>
                    {
                        var list = store.Where(f => f.AggregateId == id)
                            .Select(
                                e =>
                                    {
                                        var ret = new EntityType();
                                        copyFields(ret, e);
                                        return ret;
                                    }).AsQueryable();
                        if (take > 0)
                            list = list.Take(take);
                        return list;
                    };

            queryService.Setup(m => m.FindAggregateEntities<EntityType>(It.IsAny<string>(), It.IsAny<int>()))
                .Returns<string, int>(findById);
            queryServiceGeneric.Setup(m => m.FindAggregateEntities<EntityType>(It.IsAny<string>(), It.IsAny<int>()))
                .Returns<string, int>(findById);

            return queryService;
        }

        public static Mock<QsType> SetupQueryByBrowser<QsType, EntityType, EntityInterfaceType>(
            Mock<QsType> queryService,
            HashSet<EntityInterfaceType> store
            , MoqMockingKernel kernel
            , Action<EntityInterfaceType, EntityInterfaceType> copyFields)
            where QsType : class, QueryByBrowserInterface where EntityType : class, EntityInterfaceType, new()
            where EntityInterfaceType : class, BrowserIdInterface
        {
//            kernel.Bind<QueryByBrowserInterface>()
//                .ToConstant(queryService.Object);

            queryService.Setup(o => o.GetByBrowserId<EntityType>(It.IsAny<string>()))
            .Returns<String>(id =>
                store.Where(f => f.BrowserId == id)
                .Select(f =>
                {
                    var fli = new EntityType();
                    copyFields(fli, f);
                    return fli;
                })
                .AsQueryable());
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
//                        EntityTypeTag = typeof(EntityInterfaceType).Name,
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
//                store.Where(like => like.BrowserId == id && like.EntityTypeTag == typeof(EntityInterfaceType).Name)
//                .AsQueryable());
//            return queryService;
//        }
    }


}
