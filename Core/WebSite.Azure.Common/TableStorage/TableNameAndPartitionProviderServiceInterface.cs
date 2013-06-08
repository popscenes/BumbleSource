using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Website.Azure.Common.TableStorage
{
    public class IndexEntry
    {
                 
    }

    public interface TableNameAndPartitionProviderServiceInterface
    {

        void Add<EntityType>(string tableName,
                             Func<EntityType, string> partitionKeyFunc,
                             Func<EntityType, string> rowKeyFunc = null)
            where EntityType : class;

        void AddIndex<EntityType>(string tableName, string indexname
            , Expression<Func<EntityType, IEnumerable<StorageTableEntryInterface>>> indexEntryFactory)
                where EntityType : class;

        Func<object, string> GetPartitionKeyFunc<EntityType>();
        Func<object, string> GetRowKeyFunc<EntityType>();

        Func<object, string> GetPartitionKeyFunc(Type entityTyp);
        Func<object, string> GetRowKeyFunc(Type entityTyp);
        
        string GetTableName<EntityType>();
        string GetTableName(Type entityTyp);

        string GetTableNameForIndex<EntityType>(string indexname);
        string GetTableNameForIndex(Type entityTyp, string indexname);

        
        IEnumerable<string> GetAllTableNames();

        void SuffixTableNames(string suffix);
    }
}