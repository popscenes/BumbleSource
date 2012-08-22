//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using WebSite.Infrastructure.Domain;
//
//namespace WebSite.Azure.Common.TableStorage
//{
//    public abstract class StorageDomainEntityBase<
//          MySubClassType
//        , TableEntryType
//        , DomainEntityInterfaceType
//        , DomainEntityType> :
//            StorageDomainEntityInterface<DomainEntityInterfaceType, TableEntryType>
//        where DomainEntityInterfaceType : class
//        where TableEntryType : class, StorageTableEntryInterface<DomainEntityInterfaceType>, new()
//        where DomainEntityType : class, DomainEntityInterfaceType, new()
//        where MySubClassType : StorageDomainEntityBase<MySubClassType, TableEntryType, DomainEntityInterfaceType, DomainEntityType>, new()
//    {
//        public DomainEntityInterfaceType DomainEntity { get; protected set; }
//        protected readonly ClonedTableEntry<TableEntryType, DomainEntityInterfaceType> ClonedTable;
//        protected AzureTableContext TableContext;
//        public const int IdPartition = 0;
//        public const int AggregateIdPartition = 10;//for aggregate roots when this entity is under a root
//
//        protected StorageDomainEntityBase(
//    TableNameAndPartitionProviderInterface<DomainEntityInterfaceType> tableNameProvider)
//        {
//            ClonedTable = new ClonedTableEntry<TableEntryType, DomainEntityInterfaceType>(tableNameProvider);
//            DomainEntity = new DomainEntityType();
//        }
//
//        protected StorageDomainEntityBase(
//            TableNameAndPartitionProviderInterface<DomainEntityInterfaceType> tableNameProvider
//            , AzureTableContext tableContext)
//            : this(tableNameProvider)
//        {
//            TableContext = tableContext;
//        }
//
//        public virtual IEnumerable<StorageTableEntryInterface> GetTableEntries()
//        {
//            ClonedTable.PopulatePartitionClones<DomainEntityType>(DomainEntity, TableContext);
//            return ClonedTable.GetStorageTableEntries();
//        }
//
//        public virtual void LoadByPartition(TableEntryType table, int partition)
//        {
//            ClonedTable.SetPartitionEntity(partition, table);
//            table.UpdateEntity(DomainEntity);
//        }
//
//        public void SetContext(AzureTableContext context)
//        {
//            TableContext = context;
//        }
//
//        public static DomainEntityInterfaceType FindById(string id, AzureTableContext tableContext, int idPartition = IdPartition)
//        {
//            var tableEntry = GetTableEntry(id, tableContext, idPartition);
//            return tableEntry == null ? null : tableEntry.CreateEntityCopy<DomainEntityType, DomainEntityInterfaceType>();
//        }
//
//        public static IQueryable<DomainEntityInterfaceType> FindRelatedEntities(string myAggregateRootId
//            , AzureTableContext tableContext, int take = -1)
//        {
//            return FindEntities(tableContext, te => te.PartitionKey == myAggregateRootId, AggregateIdPartition, take);         
//        }
//
//        public static IQueryable<DomainEntityInterfaceType> FindEntities(AzureTableContext tableContext, Expression<Func<TableEntryType, bool>> query = null
//            , int partition = IdPartition, int take = -1)
//        {
//            return tableContext.PerformQuery(query, partition, take)
//            .Select(tableEntry => tableEntry.CreateEntityCopy<DomainEntityType, DomainEntityInterfaceType>());
//        }
//
//
//        public static MySubClassType GetEntityForUpdate(string id, AzureTableContext tableContext, int idPartition = IdPartition)
//        {
//            var tableEntry = GetTableEntry(id, tableContext, idPartition);
//            if (tableEntry == null)
//                return null;
//            return GetUpdateEntityForEntry(tableEntry, tableContext, idPartition);
//        }
//
//        public static IQueryable<MySubClassType> GetRelatedEntitiesForUpdate(string myAggregateRootId, AzureTableContext tableContext)
//        {
//            return GetEntitiesForUpdate(tableContext, te => te.PartitionKey == myAggregateRootId, AggregateIdPartition);
//        }
//
//        public static IQueryable<MySubClassType> GetEntitiesForUpdate(AzureTableContext tableContext, Expression<Func<TableEntryType, bool>> query = null
//        , int partition = IdPartition, int take = -1)
//        {
//            return tableContext.PerformQuery(query, partition, take)
//            .Select(tableEntry => GetUpdateEntityForEntry(tableEntry, tableContext, IdPartition));
//        }
//
//        private static TableEntryType GetTableEntry(string id, AzureTableContext tableContext, int idPartition = IdPartition)
//        {
//            var imgTableEntity = tableContext.PerformQuery<TableEntryType>(te => te.PartitionKey == id, idPartition)
//                .FirstOrDefault();
//            return imgTableEntity;
//        }
//
//        private static MySubClassType GetUpdateEntityForEntry(TableEntryType tableEntry, AzureTableContext tableContext, int idPartition)
//        {
//            var ret = new MySubClassType();
//            ret.SetContext(tableContext);
//            ret.LoadByPartition(tableEntry, idPartition);
//            return ret;
//        }
//    }
//}