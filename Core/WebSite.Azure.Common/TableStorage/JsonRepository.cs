using System;
using Website.Infrastructure.Domain;

namespace Website.Azure.Common.TableStorage
{
    public class JsonRepository
        : RepositoryBase<JsonTableEntry>
    {
        public JsonRepository(TableContextInterface tableContext
                              , TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService)
            : base(tableContext, nameAndPartitionProviderService, null)
        {
        }

        public JsonRepository(TableContextInterface tableContext
                      , TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService
                      , EntityUpdateNotificationInterface<EntityInterface> updateNotification)
            : base(tableContext, nameAndPartitionProviderService, updateNotification)
        {
        }

        //can do this with json repository as full aggregate can be reconstructed from root table entry
        protected override StorageAggregate GetEntityForUpdate(Type entity, string id) 
        {
            var root = FindById(entity, id);
            if (root == null)
                return null;
            var ret = new StorageAggregate(root, NameAndPartitionProviderService);
            ret.LoadAllTableEntriesForUpdate<JsonTableEntry>(TableContext);
            return ret;
        }
    }
}