using System;
using System.Linq;
using System.Linq.Expressions;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Website.Azure.Common.TableStorage
{
    public class QueryServiceBase<TableEntryType> 
        : GenericQueryServiceInterface
        where TableEntryType : class, StorageTableEntryInterface, new()
    {
        protected readonly TableContextInterface TableContext;
        protected readonly TableNameAndIndexProviderServiceInterface NameAndIndexProviderService;


        public QueryServiceBase(TableContextInterface tableContext, TableNameAndIndexProviderServiceInterface nameAndIndexProviderService)
        {
            TableContext = tableContext;
            NameAndIndexProviderService = nameAndIndexProviderService;
        }

        public EntityRetType FindById<EntityRetType>(string id) where EntityRetType : class, AggregateRootInterface, new()
        {
            var tableEntry = GetTableEntry<EntityRetType>(id, null);
            return tableEntry == null ? null : tableEntry.GetEntity<EntityRetType>();
        }

        public object FindById(Type entity, string id)
        {
            var tableEntry = GetTableEntry(entity, id, null);
            return tableEntry == null ? null : tableEntry.GetEntity(entity);
        }

        public EntityRetType FindByAggregate<EntityRetType>(string id, string aggregateRootId) where EntityRetType : class, AggregateInterface, new()
        {
            var tableEntry = GetTableEntry<EntityRetType>(aggregateRootId, id);
            return tableEntry == null ? null : tableEntry.GetEntity<EntityRetType>();
        }

        public object FindByAggregate(Type entity, string id, string aggregateRootId)
        {
            var tableEntry = GetTableEntry(entity, aggregateRootId, id);
            return tableEntry == null ? null : tableEntry.GetEntity(entity);
        }

        public IQueryable<string> FindAggregateEntityIds<EntityType>(string myAggregateRootId)
            where EntityType : class, AggregateInterface, new()
        {
            return FindEntityIdsByPartition<EntityType>(myAggregateRootId);
        }

        public IQueryable<string> GetAllIds<EntityRetType>() where EntityRetType : class, AggregateRootInterface, new()
        {
            return GetSelectTableEntries<EntityRetType, StorageTableKey>((Expression<Func<TableEntryType, bool>>) null,
                e => new StorageTableKey() { PartitionKey = e.PartitionKey, RowKey = e.RowKey })
                .Select(te => te.RowKey.ExtractEntityIdFromRowKey())
                .Distinct();
        }

        public IQueryable<string> GetAllIds(Type type)
        {
            return GetSelectTableEntries<StorageTableKey>(type, null,
                e => new StorageTableKey() { PartitionKey = e.PartitionKey, RowKey = e.RowKey })
                .Select(te => te.RowKey.ExtractEntityIdFromRowKey())
                .Distinct();
        }

        public IQueryable<AggregateInterface> GetAllAggregateIds<EntityRetType>() where EntityRetType : class, AggregateInterface, new()
        {
            return GetSelectTableEntries<EntityRetType, StorageTableKey>((Expression<Func<TableEntryType, bool>>)null,
            e => new StorageTableKey() { PartitionKey = e.PartitionKey, RowKey = e.RowKey })
            .Select(te => new AggregateIds() { AggregateId = te.PartitionKey, Id = te.RowKey });         
        }

        public IQueryable<EntityRetType> FindAggregateEntities<EntityRetType>(string myAggregateRootId, int take = -1) 
            where EntityRetType : class, AggregateInterface, new()
        {
            return FindEntitiesByPartition<EntityRetType>(myAggregateRootId, take);
        }

        public IQueryable<EntityRetType> FindEntitiesByPartition<EntityRetType>(string partitionKey, int take = -1)
            where EntityRetType : class, new()
        {
            return GetTableEntries<EntityRetType>(partitionKey, null, take)
                .Select(te => te.GetEntity<EntityRetType>());
        }

        public IQueryable<string> FindEntityIdsByPartition<EntityType>(string partitionKey, int take = -1)
            where EntityType : class, new()
        {
            return GetSelectTableEntries<EntityType, StorageTableKey>(partitionKey, 
                e => new StorageTableKey(){PartitionKey = e.PartitionKey, RowKey = e.RowKey}, take)
                .Select(te => te.RowKey.ExtractEntityIdFromRowKey());
        }


        protected IQueryable<SelectType> GetSelectTableEntries<EntityRetType, SelectType>(string id
            , Expression<Func<TableEntryType, SelectType>> selectExpression
            , int take = -1)
        {
            return GetSelectTableEntries<EntityRetType, SelectType>(te => te.PartitionKey == id, selectExpression,
                                                                    take);
        }

        protected IQueryable<SelectType> GetSelectTableEntries<EntityRetType, SelectType>(Expression<Func<TableEntryType, bool>> query
            , Expression<Func<TableEntryType, SelectType>> selectExpression
            , int take = -1)
        {
            var tableName = NameAndIndexProviderService.GetTableName<EntityRetType>();
            return TableContext.PerformSelectQuery(
                tableName, query, selectExpression, take: take);
        }

        protected IQueryable<SelectType> GetSelectTableEntries<SelectType>(Type entity, Expression<Func<TableEntryType, bool>> query
            , Expression<Func<TableEntryType, SelectType>> selectExpression
            , int take = -1)
        {
            var tableName = NameAndIndexProviderService.GetTableName(entity);
            return TableContext.PerformSelectQuery(
                tableName, query, selectExpression, take: take);
        }

        protected TableEntryType GetTableEntry<EntityRetType>(string id, string rowkey)
        {
            return GetTableEntries<EntityRetType>(id, rowkey, 1).FirstOrDefault();
        }

        protected TableEntryType GetTableEntry(Type entity, string partitionKey, string rowkey)
        {
            return GetTableEntries(entity, partitionKey, rowkey, 1).FirstOrDefault();
        }

        protected IQueryable<TableEntryType> GetTableEntries<EntityRetType>(string partitionkey, string rowkey, int take = -1)
        {
            var tableName = NameAndIndexProviderService.GetTableName<EntityRetType>();

            return string.IsNullOrEmpty(rowkey) 
                ? TableContext.PerformQuery<TableEntryType>(tableName, te => te.PartitionKey == partitionkey, take) 
                : TableContext.PerformQuery<TableEntryType>(tableName, te => te.PartitionKey == partitionkey && te.RowKey == rowkey, take);
        }

        protected IQueryable<TableEntryType> GetTableEntries(Type entity, string partitionkey, string rowkey, int take = -1)
        {
            var tableName = NameAndIndexProviderService.GetTableName(entity);
            return string.IsNullOrEmpty(rowkey) 
                ? TableContext.PerformQuery<TableEntryType>(tableName, te => te.PartitionKey == partitionkey, take) 
                : TableContext.PerformQuery<TableEntryType>(tableName, te => te.PartitionKey == partitionkey && te.RowKey == rowkey, take);
        }
    }
}