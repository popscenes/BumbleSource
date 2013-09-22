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
            var uow = kernel.Get<UnitOfWorkInterface>().Begin();
            using (uow)
            {
                var repository = kernel.Get<GenericRepositoryInterface>();
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

        public static void UpdateOne<EntType>(EntType entity, StandardKernel kernel
            , Action<EntType, EntType> copyFields, GenericRepositoryInterface existingUow = null)
            where EntType : class, AggregateRootInterface, new()
        {
            var uow = existingUow == null 
                ? kernel.Get<UnitOfWorkInterface>().Begin()
                : new EmptyUnitOfWork();
            using (uow)
            {
                var repository = existingUow ?? kernel.Get<GenericRepositoryInterface>();

                repository.UpdateEntity<EntType>(entity.Id, e => copyFields(e, entity));
            }
        }

        public static EntityType Get<EntityType>(string id, IKernel kernel) where EntityType : class, AggregateRootInterface, new()
        {
            var uow = kernel.Get<UnitOfWorkInterface>().Begin();
            using (uow)
            {
                var qs = kernel.Get<GenericQueryServiceInterface>();
                return qs.FindById<EntityType>(id);
            }
        }

        public static EntityType GetAggById<EntityType>(string id, string aggId, IKernel kernel) 
            where EntityType : class, AggregateInterface, new()
        {
            var uow = kernel.Get<UnitOfWorkInterface>().Begin();
            using (uow)
            {
                var qs = kernel.Get<GenericQueryServiceInterface>();
                return qs.FindByAggregate<EntityType>(id, aggId);
            }
        }

        public static IQueryable<EntityType> GetByAggRoot<EntityType>(string aggId, IKernel kernel)
            where EntityType : class, AggregateInterface, new()
        {
            var uow = kernel.Get<UnitOfWorkInterface>().Begin();
            using (uow)
            {
                var qs = kernel.Get<GenericQueryServiceInterface>();
                return qs.FindAggregateEntities<EntityType>(aggId);
            }
        }

        public static IQueryable<string> GetAll<EntityType>(string startfromid, IKernel kernel, int take) where EntityType : class, AggregateRootInterface, new()
        {
            var uow = kernel.Get<UnitOfWorkInterface>().Begin();
            using (uow)
            {
                var qs = kernel.Get<GenericQueryServiceInterface>();
                return qs.GetAllIds<EntityType>(startfromid, take);
            }
        }

        public static IList<EntityType> Get<EntityType>(IList<string> ids, IKernel kernel) where EntityType : class, AggregateRootInterface, new()
        {
                        var uow = kernel.Get<UnitOfWorkInterface>().Begin();
            using (uow)
            {
                var qs = kernel.Get<GenericQueryServiceInterface>();
                return qs.FindByIds<EntityType>(ids).ToList();
            }
        }

        public static void PerformInUow(Action<GenericRepositoryInterface> action, IKernel kernel)
        {
            var repo = kernel.Get<GenericRepositoryInterface>();
            var uow = kernel.Get<UnitOfWorkInterface>().Begin();
            using (uow)
            {
                action(repo);
            }
        }
    }
}
