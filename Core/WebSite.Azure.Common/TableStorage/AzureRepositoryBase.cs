using System;
using System.Collections.Generic;
using WebSite.Azure.Common.DataServices;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;

namespace WebSite.Azure.Common.TableStorage
{
    public abstract class AzureRepositoryBase<DomainEntityInterfaceType, DomainEntityStorageType> :
        GenericRepositoryInterface<DomainEntityInterfaceType>
        where DomainEntityInterfaceType : class
        where DomainEntityStorageType : StorageDomainEntityInterface<DomainEntityInterfaceType>
    {
        private readonly AzureTableContext _tableContext;
        private readonly EntityUpdateNotificationInterface<DomainEntityInterfaceType> _updateNotification;

        protected AzureRepositoryBase(AzureTableContext tableContext, EntityUpdateNotificationInterface<DomainEntityInterfaceType> updateNotification = null)
        {
            _tableContext = tableContext;
            _updateNotification = updateNotification;
        }

        private readonly List<Action> _mutatorsForRetry = new List<Action>();
        private readonly HashSet<DomainEntityInterfaceType> _updatedEntities = new HashSet<DomainEntityInterfaceType>(); 

        public virtual bool SaveChanges()
        {
            var ret = _tableContext.SaveChangesRetryMutatorsOnConcurrencyException(_mutatorsForRetry);
            _mutatorsForRetry.Clear();
            if (ret && _updateNotification != null)
                _updateNotification.NotifyUpdate(_updatedEntities);
            _updatedEntities.Clear();
            return ret;
        }


        public void UpdateEntity(string id, Action<DomainEntityInterfaceType> updateAction)
        {
            Action mutator = () =>
                                 {
                                     var storage = GetEntityForUpdate(id);
                                     updateAction(storage.DomainEntity);
                                     _tableContext.Store(storage.GetTableEntries());
                                     _updatedEntities.Add(storage.DomainEntity);
                                 };
            mutator();
            _mutatorsForRetry.Add(mutator);
        }

        /// <summary>
        /// For general operations that mutate the table context. Handles retry on concurrency exception
        /// </summary>
        /// <param name="mutateAction"></param>
        public void PerformMutationActionOnContext(Action<AzureTableContext> mutateAction)
        {
            Action mutator = () => mutateAction(_tableContext);
            mutator();
            _mutatorsForRetry.Add(mutator);
        }

        public virtual void Store(DomainEntityInterfaceType entity)
        {
            var tableStorageEntity = GetStorageForEntity(entity);
            _tableContext.Store(tableStorageEntity.GetTableEntries());
            _updatedEntities.Add(tableStorageEntity.DomainEntity);
        }

        public virtual void Store(object entity)
        {
            var domainEntity = entity as DomainEntityInterfaceType;
            if (domainEntity != null)
                Store(domainEntity);
        }

        protected abstract DomainEntityStorageType GetEntityForUpdate(string id);
        protected abstract DomainEntityStorageType GetStorageForEntity(DomainEntityInterfaceType entity);

    }
}