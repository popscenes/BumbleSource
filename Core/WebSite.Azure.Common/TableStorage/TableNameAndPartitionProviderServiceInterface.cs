using System;
using System.Collections.Generic;

namespace Website.Azure.Common.TableStorage
{
    public interface TableNameAndPartitionProviderServiceInterface
    {
        void Add<EntityType>(int partitionId, string tableName,
                             Func<EntityType, string> partitionKeyFunc,
                             Func<EntityType, string> rowKeyFunc = null)
            where EntityType : class;
        int[] GetPartitionIdentifiers<EntityType>();
        int[] GetPartitionIdentifiers(Type entityTyp);

        Func<object, string> GetPartitionKeyFunc<EntityType>(int partition);
        Func<object, string> GetRowKeyFunc<EntityType>(int partition);

        Func<object, string> GetPartitionKeyFunc(Type entityTyp, int partition);
        Func<object, string> GetRowKeyFunc(Type entityTyp, int partition);
        
        string GetTableName<EntityType>(int partition);
        string GetTableName(Type entityTyp, int partition);
        
        IEnumerable<string> GetAllTableNames();

        void SuffixTableNames(string suffix);
    }
}