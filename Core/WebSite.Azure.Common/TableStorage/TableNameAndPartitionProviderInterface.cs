//using System;
//using System.Collections.Generic;
//
//namespace WebSite.Azure.Common.TableStorage
//{
//    public interface TableNameAndPartitionProviderInterface
//    {
//        IEnumerable<KeyValuePair<Type, string>> GetAllTableTypesAndNames();
//        string GetTableName(Type type, int partition);
//        int GetPartitionCount(Type type);
//        int[] GetPartitionIdentifiers(Type type);
//    }
//
//    public interface TableNameAndPartitionProviderInterface<in EntityType> : TableNameAndPartitionProviderInterface
//    {
//        Func<EntityType, string> GetPartitionKeyFunc(Type type, int partition);
//        Func<EntityType, string> GetRowKeyFunc(Type type, int partition);
//    }
//}