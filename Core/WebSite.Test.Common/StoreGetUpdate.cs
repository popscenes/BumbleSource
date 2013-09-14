using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Ninject;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Website.Test.Common
{
    public static class StoreGetUpdate
    {
        public static void Store<EntType>(EntType entity, IKernel kernel)
        {
            var repository = kernel.Get<GenericRepositoryInterface>();
            var uow = kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository });
            using (uow)
            {

                repository.Store(entity);
            }

            Assert.IsTrue(uow.Successful);
        }

        public static void StoreAll<EntType>(IList<EntType> entities, IKernel kernel)
        {
            foreach (var entity in entities)
            {
                Store(entity, kernel);
            }
        }

        internal static void UpdateOne<EntType>(EntType entity, GenericRepositoryInterface repository, StandardKernel kernel
            , Action<EntType, EntType> copyFields)
            where EntType : class, AggregateRootInterface, new()
        {
            EntType oldState = null;
            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = kernel.Get<UnitOfWorkFactoryInterface>().GetUnitOfWork(new List<RepositoryInterface>() { repository }))
            {
                repository.UpdateEntity<EntType>(entity.Id, e =>
                {
                    oldState = e.CreateCopy<EntType, EntType>(copyFields);
                    e.CopyFieldsFrom(entity);
                });
            }
        }

        public static EntityType Get<EntityType>(string id, IKernel kernel) where EntityType : class, AggregateRootInterface, new()
        {
            var qs = kernel.Get<GenericQueryServiceInterface>();
            return qs.FindById<EntityType>(id);
        }

        public static IList<EntityType> Get<EntityType>(IList<string> ids, IKernel kernel) where EntityType : class, AggregateRootInterface, new()
        {
            var qs = kernel.Get<GenericQueryServiceInterface>();
            return qs.FindByIds<EntityType>(ids).ToList();
        }

        public static void PerformInUow(Action<GenericRepositoryInterface> action, IKernel kernel)
        {
            var repo = kernel.Get<GenericRepositoryInterface>();
            using (var uow = kernel.Get<UnitOfWorkFactoryInterface>().GetUnitOfWork(repo))
            {
                action(repo);
            }
        }
    }
}
