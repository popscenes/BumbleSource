using System;
using System.Collections.Generic;
using System.Linq;
using Website.Azure.Common.DataServices;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace Website.Azure.Common.TableStorage
{
    public abstract class RepositoryBase<TableEntryType> : 
        QueryServiceBase<TableEntryType>,
        GenericRepositoryInterface
        where TableEntryType : class, StorageTableEntryInterface, new()
    {


        protected RepositoryBase(TableContextInterface tableContext,
                                 TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService) 
            : base(tableContext, nameAndPartitionProviderService)
        {
        }

        private readonly List<Action> _mutatorsForRetry = new List<Action>();

        public virtual bool SaveChanges()
        {
            var ret = TableContext.SaveChangesRetryMutatorsOnConcurrencyException(_mutatorsForRetry);
            _mutatorsForRetry.Clear();
            return ret;
        }

        public void UpdateEntity<UpdateType>(string id, Action<UpdateType> updateAction) where UpdateType : class, EntityIdInterface, new()
        {
            Action mutator = () =>
                                 {
                                     var storage = GetEntityForUpdate<UpdateType>(id);
                                     if (storage == null) return;
                                     updateAction((UpdateType)storage.AggregateRoot);
                                     StoreAggregate(storage);
                                 };
            mutator();
            _mutatorsForRetry.Add(mutator);
        }

        public void UpdateEntity(Type entityTyp, string id, Action<object> updateAction)
        {
            Action mutator = () =>
            {
                var storage = GetEntityForUpdate(entityTyp, id);
                if (storage == null) return;
                updateAction(storage.AggregateRoot);
                StoreAggregate(storage);
            };
            mutator();
            _mutatorsForRetry.Add(mutator);
        }

        protected StorageAggregate GetEntityForUpdate<UpdateType>(string id) where UpdateType : class, new()
        {
            return GetEntityForUpdate(typeof (UpdateType), id);
        }

        protected abstract StorageAggregate GetEntityForUpdate(Type entity, string id);

        /// <summary>
        /// For general operations that mutate the table context. Handles retry on concurrency exception
        /// </summary>
        /// <param name="mutateAction"></param>
        public void PerformMutationActionOnContext(Action<TableContextInterface> mutateAction)
        {
            Action mutator = () => mutateAction(TableContext);
            mutator();
            _mutatorsForRetry.Add(mutator);
        }

        public virtual void Store<EntityType>(EntityType entity)
        {
            var storageAggregate = new StorageAggregate(entity, NameAndPartitionProviderService);
            StoreAggregate(storageAggregate);
        }

        public virtual void Store(object entity)
        {
            Store<object>(entity);
        }

        private void StoreAggregate(StorageAggregate aggregate)
        {
            var deleted = false;
            var tableEntries = aggregate.GetTableEntries<TableEntryType>(TableContext);
            foreach (var tableEntry in tableEntries.Where(e => e.Deleted || e.Entry.KeyChanged))
            {
                deleted = true;
                TableContext.Delete(tableEntry.Entry);
                tableEntry.Entry.KeyChanged = false;
            }

            if (deleted)
                TableContext.SaveChanges();

            foreach (var tableEntry in tableEntries.Where(e => !e.Deleted))
            {
                TableContext.Store(tableEntry.TableName, tableEntry.Entry);
            }
        }


    }
}