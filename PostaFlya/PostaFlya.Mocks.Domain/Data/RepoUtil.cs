using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Moq;
using Ninject.MockingKernel.Moq;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Query;
using PostaFlya.Domain.Comments;
using PostaFlya.Domain.Comments.Command;
using PostaFlya.Domain.Comments.Query;
using PostaFlya.Domain.Likes;
using PostaFlya.Domain.Likes.Command;
using PostaFlya.Domain.Likes.Query;
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

        public static bool CopyAndStore<EntityType, EntityInterfaceType>(this HashSet<EntityInterfaceType> store, EntityInterfaceType source, Action<EntityInterfaceType, EntityInterfaceType> copyFields)
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
            where RepoType : class, GenericRepositoryInterface<EntityInterfaceType> where EntityType : class, EntityInterfaceType, new()
            where EntityInterfaceType : EntityInterface
        {
            var repository = kernel.GetMock<RepoType>();
            kernel.Bind<RepoType>()
                .ToConstant(repository.Object).InSingletonScope();
            kernel.Bind<GenericRepositoryInterface<EntityInterfaceType>>()
                .ToConstant(repository.Object).InSingletonScope();

            repository.Setup(o => o.Store(It.IsAny<EntityInterfaceType>()))
                .Callback<EntityInterfaceType>(entity => store.CopyAndStore<EntityType, EntityInterfaceType>(entity, copyFields));

            repository.Setup(m => m.UpdateEntity(It.IsAny<string>(), It.IsAny<Action<EntityInterfaceType>>()))
                .Callback<string, Action<EntityInterfaceType>>((id, act) =>
                                 act(store.SingleOrDefault(b => b.Id == id)));

            //to simulate failure set entity id to null
            repository.Setup(r => r.SaveChanges()).Returns(() => store.All(e => !string.IsNullOrWhiteSpace(e.Id)));

            return repository;
        }

        public static Mock<QsType> SetupQueryService<QsType, EntityType, EntityInterfaceType>(
            HashSet<EntityInterfaceType> store
            , MoqMockingKernel kernel
            , Action<EntityInterfaceType, EntityInterfaceType> copyFields)
            where QsType : class, GenericQueryServiceInterface<EntityInterfaceType> where EntityType : class, EntityInterfaceType, new()
            where EntityInterfaceType : class, EntityInterface
        {
            var queryService = kernel.MockRepository.Create<QsType>();
            var queryServiceBase = queryService.As<QueryServiceInterface>();

            kernel.Bind<QsType>()
                .ToConstant(queryService.Object);
            kernel.Bind<GenericQueryServiceInterface<EntityInterfaceType>>()
                .ToConstant(queryService.Object);

            Func<string, EntityInterfaceType> findById =
                id =>
                    {
                        //create a copy for find by id
                        var entity = new EntityType();
                        var stored = store.SingleOrDefault(f => f.Id == id);
                        if (stored == null) return null;
                        copyFields(entity, stored);
                        return entity;
                    };

            queryService.Setup(m => m.FindById(It.IsAny<string>()))
                .Returns<string>(findById);
            queryServiceBase.Setup(m => m.FindById(It.IsAny<string>()))
                .Returns<string>(findById);

            return queryService;
        }

        public static Mock<QsType> SetupQueryByBrowser<QsType, EntityType, EntityInterfaceType>(
            Mock<QsType> queryService,
            HashSet<EntityInterfaceType> store
            , MoqMockingKernel kernel
            , Action<EntityInterfaceType, EntityInterfaceType> copyFields)
            where QsType : class, QueryByBrowserInterface<EntityInterfaceType> where EntityType : class, EntityInterfaceType, new()
            where EntityInterfaceType : class, BrowserIdInterface
        {
            kernel.Bind<QueryByBrowserInterface<EntityInterfaceType>>()
                .ToConstant(queryService.Object);

            queryService.Setup(o => o.GetByBrowserId(It.IsAny<string>()))
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

        public static Mock<QsType> SetupQueryComments<QsType, EntityInterfaceType>(
            Mock<QsType> queryService,
            HashSet<CommentInterface> store
            , MoqMockingKernel kernel)
            where QsType : class, QueryCommentsInterface<EntityInterfaceType> where EntityInterfaceType : class, BrowserIdInterface, CommentableInterface
        {
            kernel.Bind<QueryCommentsInterface<EntityInterfaceType>>()
                .ToConstant(queryService.Object);

            queryService.Setup(o => o.GetComments(It.IsAny<string>(), It.IsAny<int>()))
            .Returns<String, int>((id, take) =>
                store.Where(c => c.EntityId == id)
                .Select(c =>
                {
                    var com = new Comment()
                                  {
                                      BrowserId = c.BrowserId,
                                      EntityId = c.EntityId,
                                      CommentContent = c.CommentContent
                                  };
                    return com;
                }).Take( take == -1 ? store.Count : take)
                .AsQueryable());
            return queryService;
        }


        internal static Mock<RepoType> SetupAddComment<RepoType, EntityInterfaceType>(
            Mock<RepoType> repository,
            HashSet<CommentInterface> store
            , MoqMockingKernel kernel)
            where RepoType : class, AddCommentInterface<EntityInterfaceType>, GenericRepositoryInterface<EntityInterfaceType>
            where EntityInterfaceType : class, CommentableInterface
        {
            kernel.Bind<AddCommentInterface<EntityInterfaceType>>()
                .ToConstant(repository.Object);

            repository.Setup(o => o.AddComment(It.IsAny<CommentInterface>()))
                .Returns<CommentInterface>(c =>
                {
                    CommentableInterface entity = null;
                    repository.Object.UpdateEntity(c.EntityId, e =>
                    {
                        entity = e;
                        if(entity != null)
                            e.NumberOfComments++;
                    });
                    var com = new Comment();
                    com.CopyFieldsFrom(c);
                    com.Id = com.Id ?? Guid.NewGuid().ToString();
                    store.Add(com);
                    return store.Any(comment => string.IsNullOrWhiteSpace(comment.Id)) ? null : entity;
                });

            return repository;
        }


        internal static Mock<RepoType> SetupAddLike<RepoType, EntityInterfaceType>(
            Mock<RepoType> repository,
            IList<LikeInterface> store
            , MoqMockingKernel kernel)
            where RepoType : class, AddLikeInterface<EntityInterfaceType>, GenericRepositoryInterface<EntityInterfaceType> where EntityInterfaceType : class, LikeableInterface
        {
            kernel.Bind<AddLikeInterface<EntityInterfaceType>>()
                .ToConstant(repository.Object);

            repository.Setup(o => o.Like(It.IsAny<LikeInterface>()))
                .Returns<LikeInterface>((l) =>
                {
                    
                    var like = store.SingleOrDefault(ls =>
                        ls.BrowserId.Equals(l.BrowserId)
                        && ls.EntityId == l.EntityId);

                    LikeableInterface entity = null;
                    if (like != null)
                        store.Remove(like);

                    repository.Object.UpdateEntity(l.EntityId
                    , e =>
                            {
                                entity = e;
                                if(like == null)
                                    e.NumberOfLikes++;
                            });
                    

                    var newl = new Like()
                    {
                        EntityTypeTag = typeof(EntityInterfaceType).Name,
                        BrowserId = l.BrowserId,
                        LikeContent = l.LikeContent,
                        EntityId = l.EntityId,
                        ILike = l.ILike,
                    };
                    store.Add(newl);

                    return entity;
                });

            return repository;
        }

        public static Mock<QsType> SetupQueryLike<QsType, EntityInterfaceType>(
            Mock<QsType> queryService,
            IList<LikeInterface> store
            , MoqMockingKernel kernel)
            where QsType : class, QueryLikesInterface<EntityInterfaceType>, GenericQueryServiceInterface<EntityInterfaceType>
            where EntityInterfaceType : class, LikeableInterface
        {
            kernel.Bind<QueryLikesInterface<EntityInterfaceType>>()
                .ToConstant(queryService.Object);

            queryService.Setup(o => o.GetLikes(It.IsAny<string>()))
            .Returns<String>(id =>
                store.Where(like => like.EntityId == id)
                .Select(like =>
                {
                    var lik = new Like();
                    lik.CopyFieldsFrom(like);
                    return lik;
                })
                .AsQueryable());

            queryService.Setup(o => o.GetLikesByBrowser(It.IsAny<string>()))
            .Returns<String>(id =>
                store.Where(like => like.BrowserId == id && like.EntityTypeTag == typeof(EntityInterfaceType).Name)
                .AsQueryable());
            return queryService;
        }
    }


}
