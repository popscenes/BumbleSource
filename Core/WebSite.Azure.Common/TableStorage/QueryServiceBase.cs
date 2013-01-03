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
        protected readonly TableNameAndPartitionProviderServiceInterface NameAndPartitionProviderService;
        public const int IdPartition = 0;
        public const int FriendlyIdPartiton = 1;
        public const int AggregateIdPartition = 10;


        public QueryServiceBase(TableContextInterface tableContext, TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService)
        {
            TableContext = tableContext;
            NameAndPartitionProviderService = nameAndPartitionProviderService;
        }

        public EntityRetType FindById<EntityRetType>(string id) 
            where EntityRetType : class, new()
        {
            return FindById<EntityRetType>(id, IdPartition);
        }

        public object FindById(Type entity, string id)
        {
            var tableEntry = GetTableEntry(entity, id, IdPartition);
            return tableEntry == null ? null : tableEntry.GetEntity(entity);
        }

        public IQueryable<string> FindAggregateEntityIds<EntityType>(string myAggregateRootId)
            where EntityType : class, AggregateInterface, new()
        {
            return FindEntityIdsByPartition<EntityType>(myAggregateRootId, AggregateIdPartition);
        }

        public IQueryable<string> GetAllIds<EntityRetType>() where EntityRetType : class, new()
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

        public EntityRetType FindByFriendlyId<EntityRetType>(string id) where EntityRetType : class, new()
        {
            return FindById<EntityRetType>(id, FriendlyIdPartiton);
        }

        public object FindByFriendlyId(Type entity, string id)
        {
            var tableEntry = GetTableEntry(entity, id, FriendlyIdPartiton);
            return tableEntry == null ? null : tableEntry.GetEntity(entity);
        }

        protected EntityRetType FindById<EntityRetType>(string id, int partitionId)
            where EntityRetType : class, new()
        {
            var tableEntry = GetTableEntry<EntityRetType>(id, partitionId);
            return tableEntry == null ? null : tableEntry.GetEntity<EntityRetType>();
        }

        public IQueryable<EntityRetType> FindAggregateEntities<EntityRetType>(string myAggregateRootId, int take = -1) 
            where EntityRetType : class, AggregateInterface, new()
        {
            return FindEntitiesByPartition<EntityRetType>(myAggregateRootId, AggregateIdPartition, take);
        }

        public IQueryable<EntityRetType> FindEntitiesByPartition<EntityRetType>(string partitionKey, int partitionId, int take = -1)
            where EntityRetType : class, new()
        {
            return GetTableEntries<EntityRetType>(partitionKey, partitionId, take)
                .Select(te => te.GetEntity<EntityRetType>());
        }

        public IQueryable<string> FindEntityIdsByPartition<EntityType>(string partitionKey, int partitionId, int take = -1)
            where EntityType : class, new()
        {
            return GetSelectTableEntries<EntityType, StorageTableKey>(partitionKey, 
                e => new StorageTableKey(){PartitionKey = e.PartitionKey, RowKey = e.RowKey},
                partitionId, take)
                .Select(te => te.RowKey.ExtractEntityIdFromRowKey());
        }


        protected IQueryable<SelectType> GetSelectTableEntries<EntityRetType, SelectType>(string id
            , Expression<Func<TableEntryType, SelectType>> selectExpression
            , int idPartition = 0, int take = -1)
        {
            return GetSelectTableEntries<EntityRetType, SelectType>(te => te.PartitionKey == id, selectExpression,
                                                                    idPartition, take);
        }

        protected IQueryable<SelectType> GetSelectTableEntries<EntityRetType, SelectType>(Expression<Func<TableEntryType, bool>> query
            , Expression<Func<TableEntryType, SelectType>> selectExpression
            , int idPartition = 0, int take = -1)
        {
            var tableName = NameAndPartitionProviderService.GetTableName<EntityRetType>(IdPartition);
            return TableContext.PerformSelectQuery(
                tableName, query, selectExpression, idPartition, take);
        }

        protected IQueryable<SelectType> GetSelectTableEntries<SelectType>(Type entity, Expression<Func<TableEntryType, bool>> query
            , Expression<Func<TableEntryType, SelectType>> selectExpression
            , int idPartition = 0, int take = -1)
        {
            var tableName = NameAndPartitionProviderService.GetTableName(entity, IdPartition);
            return TableContext.PerformSelectQuery(
                tableName, query, selectExpression, idPartition, take);
        }

        protected TableEntryType GetTableEntry<EntityRetType>(string id, int idPartition = 0)
        {
            return GetTableEntries<EntityRetType>(id, idPartition, 1).FirstOrDefault();
        }

        protected TableEntryType GetTableEntry(Type entity, string id, int idPartition = 0)
        {
            return GetTableEntries(entity, id, idPartition, 1).FirstOrDefault();
        }

        protected IQueryable<TableEntryType> GetTableEntries<EntityRetType>(string id, int idPartition = 0, int take = -1)
        {
            var tableName = NameAndPartitionProviderService.GetTableName<EntityRetType>(IdPartition);
            return TableContext.PerformQuery<TableEntryType>(tableName, te => te.PartitionKey == id, idPartition, take);
        }

        protected IQueryable<TableEntryType> GetTableEntries(Type entity, string id, int idPartition = 0, int take = -1)
        {
            var tableName = NameAndPartitionProviderService.GetTableName(entity, IdPartition);
            return TableContext.PerformQuery<TableEntryType>(tableName, te => te.PartitionKey == id, idPartition, take);
        }
    }
}