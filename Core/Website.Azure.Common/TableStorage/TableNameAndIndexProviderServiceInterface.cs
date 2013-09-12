using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Website.Infrastructure.Query;

namespace Website.Azure.Common.TableStorage
{
    public class IndexEntry
    {
                 
    }

    public abstract class Index<EntityQueryType>
    {
        public abstract string IndexName { get; }
        public Type TypeForQuery { get { return typeof(EntityQueryType); } }
    }

    public abstract class IndexDefinition<EntityQueryType, EntityIndexType>
        : Index<EntityQueryType>
     where EntityIndexType : class, EntityQueryType
    {
        public abstract Expression<Func<QueryChannelInterface, EntityIndexType, IEnumerable<StorageTableKeyInterface>>> Definition { get; }
        public Type TypeForIndex { get { return typeof(EntityIndexType); } }
    }


    public interface TableNameAndIndexProviderServiceInterface
    {
        //note partitionKeyFunc and rowKeyFunc should not return different values in AggregateRootInterface entities
        void Add<EntityType>(string tableName,
                             Func<EntityType, string> partitionKeyFunc,
                             Func<EntityType, string> rowKeyFunc = null)
            where EntityType : class;


        /// <summary>
        /// Note.. only use volatile partition keys, make sure that row keys will not change for an entity in index otherwise
        /// they can't be located when being updated
        /// ideally row key should just be [entityId]
        /// </summary>
        void AddIndex<EntityQueryType, EntityIndexType>(string tableName, IndexDefinition<EntityQueryType, EntityIndexType> definition)
                where EntityIndexType : class, EntityQueryType;

        Func<object, string> GetPartitionKeyFunc<EntityType>();
        Func<object, string> GetRowKeyFunc<EntityType>();

        Func<object, string> GetPartitionKeyFunc(Type entityTyp);
        Func<object, string> GetRowKeyFunc(Type entityTyp);
        
        string GetTableName<EntityType>();
        string GetTableName(Type entityTyp);
 
        string GetTableNameForIndex<EntityType>(string indexname);
        string GetTableNameForIndex(Type entityTyp, string indexname);
        IEnumerable<string> GetAllIndexNamesForUpdate<EntityType>();
        Func<QueryChannelInterface, object, IEnumerable<StorageTableKeyInterface>> GetIndexEntryFactoryFor<EntityType>(string indexname);

        IEnumerable<string> GetAllTableNames();

        void SuffixTableNames(string suffix);
    }
}