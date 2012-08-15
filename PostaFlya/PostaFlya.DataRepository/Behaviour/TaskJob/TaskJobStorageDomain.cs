using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure.StorageClient;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Content;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.TaskJob;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Util;

namespace PostaFlya.DataRepository.Behaviour.TaskJob
{
    internal class TaskJobTableEntry : ExtendableTableEntry,
        StorageTableEntryInterface<TaskJobFlierBehaviourInterface>
    {
        public double MaxAmount { get; set; }
        public double CostOverhead { get; set; }
        public byte[] ExtraLocations { get; set; }

        public void Update(TaskJobFlierBehaviourInterface source)
        {
            ExtraLocations = SerializeUtil.ToByteArray(source.ExtraLocations);
            MaxAmount = source.MaxAmount;
            CostOverhead = source.CostOverhead;
#if DEBUG
            UpdateEntity(source);
#endif
        }

        public void UpdateEntity(TaskJobFlierBehaviourInterface target)
        {
            target.ExtraLocations = SerializeUtil.FromByteArray<Locations>(ExtraLocations);
            target.MaxAmount = MaxAmount;
            target.CostOverhead = CostOverhead;
            target.Id = RowKey;
        }
    }


    internal class TaskJobStorageDomain : 
        TaskJobFlierBehaviour, StorageDomainEntityInterface<TaskJobFlierBehaviourInterface>
    {
        public static TableNameAndPartitionProvider<TaskJobFlierBehaviourInterface> 
            TableNamesAndPartition = new TableNameAndPartitionProvider<TaskJobFlierBehaviourInterface>()
                                                         {
                                                           {typeof(TaskJobTableEntry), 0, "taskjob", behaviour => behaviour.Id, behaviour => behaviour.Id}    
                                                         };

        private readonly ClonedTableEntry<TaskJobTableEntry, TaskJobFlierBehaviourInterface>
            _clonedTable = new ClonedTableEntry<TaskJobTableEntry, TaskJobFlierBehaviourInterface>(TableNamesAndPartition);

        private readonly AzureTableContext _tableContext;
        public TaskJobStorageDomain(AzureTableContext tableContext)
        {
            _tableContext = tableContext;
        }

        public TaskJobStorageDomain(TaskJobFlierBehaviourInterface source, AzureTableContext tableContext)
            : this(tableContext)
        {
            this.CopyFieldsFrom(source);
            _clonedTable.CreateDefaultEntries();
        }


        public IEnumerable<StorageTableEntryInterface> GetTableEntries()
        {
            _clonedTable.PopulatePartitionClones <TaskJobFlierBehaviour>(this, _tableContext);
            return _clonedTable.GetStorageTableEntries();
        }

        public void LoadByPartition(TaskJobTableEntry table, int partition = 0)
        {
            _clonedTable.SetPartitionEntity(partition, table);
            table.UpdateEntity(this);
        }

        public static TaskJobFlierBehaviourInterface FindById(string id, AzureTableContext tableContext)
        {
            var tableEntity =
                tableContext.PerformQuery<TaskJobTableEntry>(e => e.PartitionKey == id && e.RowKey == id)
                .SingleOrDefault();

            if (tableEntity == null)
                return null;

            var ret = new TaskJobFlierBehaviour();
            tableEntity.UpdateEntity(ret);
            return ret;
        }

        public static TaskJobStorageDomain GetEntityForUpdate(string id, AzureTableContext tableContext)
        {
            var tableEntity =
                tableContext.PerformQuery<TaskJobTableEntry>(e => e.PartitionKey == id && e.RowKey == id)
                .SingleOrDefault();

            if (tableEntity == null)
                return null;

            var ret = new TaskJobStorageDomain(tableContext);
            ret.LoadByPartition(tableEntity);
            return ret;
        }

        public TaskJobFlierBehaviourInterface DomainEntity
        {
            get { return this; }
        }
    }
}