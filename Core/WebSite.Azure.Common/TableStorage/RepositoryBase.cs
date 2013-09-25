using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using Website.Azure.Common.DataServices;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;

namespace Website.Azure.Common.TableStorage
{
    public abstract class RepositoryBase<TableEntryType> : 
        QueryServiceBase<TableEntryType>,
        GenericRepositoryInterface
        where TableEntryType : class, StorageTableEntryInterface, new()
    {
        private readonly EventPublishServiceInterface _publishService;


        protected RepositoryBase(TableContextInterface tableContext,
                                 TableNameAndIndexProviderServiceInterface nameAndIndexProviderService,
            EventPublishServiceInterface publishService) 
            : base(tableContext, nameAndIndexProviderService)
        {
            _publishService = publishService;
        }

        private readonly List<Action> _mutatorsForRetry = new List<Action>();
        private readonly List<EventInterface> _updateEvents = new List<EventInterface>();

        public virtual bool SaveChanges()
        {
            _mutatorsForRetry.Insert(0, () => _updateEvents.Clear());
            var ret = TableContext.SaveChangesRetryOnException(_mutatorsForRetry);
            _mutatorsForRetry.Clear();
            _publishService.PublishAll(_updateEvents);
            _updateEvents.Clear();
            
            return ret;
        }

        public void UpdateEntity<UpdateType>(string id, Action<UpdateType> updateAction) where UpdateType : class, AggregateRootInterface, new()
        {
            Action mutator = () =>
                                 {
                                     var storage = GetTableEntry<UpdateType>(id, null);
                                     if (storage == null) return;
                                     updateAction(storage.GetEntity<UpdateType>());
                                     storage.UpdateEntry();
                                     StoreAggregate(storage);
                                     _updateEvents.Add(new EntityModifiedEvent<UpdateType>()
                                        {
                                            Entity = storage.GetEntity<UpdateType>()
                                        });
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
                storage.UpdateEntry();
                StoreAggregate(storage);
                _updateEvents.Add(new EntityModifiedEvent<UpdateType>()
                {
                    Entity = storage.GetEntity<UpdateType>()
                });
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
                storage.UpdateEntry();
                StoreAggregate(storage);
                _updateEvents.Add(EntityModifiedEventCreator.CreateFor(storage.GetEntity(entityTyp)));
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
                storage.UpdateEntry();
                StoreAggregate(storage);
                _updateEvents.Add(EntityModifiedEventCreator.CreateFor(storage.GetEntity(entityTyp)));

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
            _updateEvents.Add(EntityModifiedEventCreator.CreateFor(entity));

        }

        public virtual void Store(object entity)
        {
            Store<object>(entity);
        }

        private void StoreAggregate(StorageTableEntryInterface aggregate)
        {
            var type = aggregate.GetEntity().GetType();
            var partKeyFunc = NameAndIndexProviderService.GetPartitionKeyFunc(type);
            var rowKeyFunc = NameAndIndexProviderService.GetRowKeyFunc(type);
            var rowKey = rowKeyFunc(aggregate.GetEntity());
            var partitionKey = partKeyFunc(aggregate.GetEntity());
            aggregate.KeyChanged = (!string.IsNullOrWhiteSpace(aggregate.RowKey) &&
                                            (aggregate.RowKey != rowKey ||
                                            aggregate.PartitionKey != partitionKey));
            aggregate.RowKey = rowKey;
            aggregate.PartitionKey = partitionKey;

            var deleted = false;
            if(aggregate.KeyChanged)
            {
                deleted = true;
                TableContext.Delete(aggregate);
                aggregate.KeyChanged = false;
            }

            if (deleted)
                TableContext.SaveChanges();

            TableContext.Store(NameAndIndexProviderService.GetTableName(aggregate.GetEntity().GetType()),aggregate);
            
        }


    }
}