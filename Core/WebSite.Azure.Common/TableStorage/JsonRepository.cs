using System;
using Website.Infrastructure.Domain;

namespace Website.Azure.Common.TableStorage
{
    public class JsonRepository
        : RepositoryBase<JsonTableEntry>
    {
        public JsonRepository(TableContextInterface tableContext
                      , TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService)
            : base(tableContext, nameAndPartitionProviderService)
        {
        }
    }
}