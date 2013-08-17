using System;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;

namespace Website.Azure.Common.TableStorage
{
    public class JsonRepository
        : RepositoryBase<JsonTableEntry>
    {
        public JsonRepository(TableContextInterface tableContext
                      , TableNameAndIndexProviderServiceInterface nameAndIndexProviderService
                    , EventPublishServiceInterface publishService)
            : base(tableContext, nameAndIndexProviderService, publishService)
        {
        }
    }
}