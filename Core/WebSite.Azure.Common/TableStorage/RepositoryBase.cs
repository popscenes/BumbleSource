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

        public void UpdateEntity<UpdateType>(string id, Action<UpdateType> updateAction) where UpdateType : class, AggregateRootInterface, new()
        {
            Action mutator = () =>
                                 {
                                     var storage = GetTableEntry<UpdateType>(id, null);
                                     if (storage == null) return;
                                     updateAction(storage.GetEntity<UpdateType>());
                                     StoreAggregate(storage);
                                 };
            mutator();
            _mutatorsForRetry.Add(mutator);
        }

        public void UpdateAggregateEntity<UpdateType>(string id, string aggregateRootId, Action<UpdateType> updateAction) where UpdateType : class, AggregateInterface, new()
        {
            Action mutator = () =>
            {
                var storage = GetTableEntry<UpdateType>(aggregateRootId, id);
                if (storage == null) return;
                updateAction(storage.GetEntity<UpdateType>());
                StoreAggregate(storage);
            };
            mutator();
            _mutatorsForRetry.Add(mutator);
        }

        public void UpdateEntity(Type entityTyp, string id, Action<object> updateAction)
        {
            Action mutator = () =>
            {
                var storage = GetTableEntry(entityTyp, id, null);
                if (storage == null) return;
                updateAction(storage.GetEntity(entityTyp));
                StoreAggregate(storage);
            };
            mutator();
            _mutatorsForRetry.Add(mutator);
        }

        public void UpdateAggregateEntity(Type entityTyp, string id, string aggregateRootId, Action<object> updateAction)
        {
            Action mutator = () =>
            {
                var storage = GetTableEntry(entityTyp, aggregateRootId, id);
                if (storage == null) return;
                updateAction(storage.GetEntity(entityTyp));
                StoreAggregate(storage);
            };
            mutator();
            _mutatorsForRetry.Add(mutator);
        }


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
            var tableStorage = new TableEntryType();
            tableStorage.Init(entity);
            StoreAggregate(tableStorage);
        }

        public virtual void Store(object entity)
        {
            Store<object>(entity);
        }

        private void StoreAggregate(StorageTableEntryInterface aggregate)
        {
            var deleted = false;
            if(aggregate.KeyChanged)
            {
                deleted = true;
                TableContext.Delete(aggregate);
                aggregate.KeyChanged = false;
            }

            if (deleted)
                TableContext.SaveChanges();

            TableContext.Store(NameAndPartitionProviderService.GetTableName(aggregate.GetEntity().GetType()),aggregate);
            
        }


    }
}