using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Website.Azure.Common.TableStorage
{
    public class IndexEntry
    {
                 
    }

    public static class TableNameAndIndexProviderServiceInterfaceExtensions
    {
        public static void AddIndex<EntityIndexType>(this TableNameAndIndexProviderServiceInterface tip
            ,string tableName, string indexname
            , Expression<Func<EntityIndexType, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory)
            where EntityIndexType : class
        {
            tip.AddIndex<EntityIndexType, EntityIndexType>(tableName, indexname, indexEntryFactory);
        }
    }

    public interface TableNameAndIndexProviderServiceInterface
    {
        //note partitionKeyFunc and rowKeyFunc should not return different values in AggregateRootInterface entities
        void Add<EntityType>(string tableName,
                             Func<EntityType, string> partitionKeyFunc,
                             Func<EntityType, string> rowKeyFunc = null)
            where EntityType : class;


        void AddIndex<EntityQueryType, EntityIndexType>(string tableName, string indexname
            , Expression<Func<EntityIndexType, IEnumerable<StorageTableKeyInterface>>> indexEntryFactory)
                where EntityIndexType : class, EntityQueryType;

        Func<object, string> GetPartitionKeyFunc<EntityType>();
        Func<object, string> GetRowKeyFunc<EntityType>();

        Func<object, string> GetPartitionKeyFunc(Type entityTyp);
        Func<object, string> GetRowKeyFunc(Type entityTyp);
        
        string GetTableName<EntityType>();
        string GetTableName(Type entityTyp);
 
        string GetTableNameForIndex<EntityType>(string indexname);
        string GetTableNameForIndex(Type entityTyp, string indexname);
        IEnumerable<string> GetAllIndexNamesFor<EntityType>();
        Func<object, IEnumerable<StorageTableKeyInterface>> GetIndexEntryFactoryFor<EntityType>(string indexname);

        IEnumerable<string> GetAllTableNames();

        void SuffixTableNames(string suffix);
    }
}