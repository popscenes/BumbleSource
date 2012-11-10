using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Domain;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;

namespace PostaFlya.DataRepository.Internal
{
    internal class JsonRepositoryWithBrowser : JsonRepository,
        QueryServiceForBrowserAggregateInterface
    {
        public const int BrowserPartitionId = 11;
        public JsonRepositoryWithBrowser(TableContextInterface tableContext
            , TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService)
            : base(tableContext, nameAndPartitionProviderService)
        {
        }

        public IQueryable<string> GetEntityIdsByBrowserId<EntityType>(String browserId) where EntityType : class, BrowserIdInterface, new()
        {
            if (string.IsNullOrWhiteSpace(browserId))
                return null;

            return FindEntityIdsByPartition<EntityType>(browserId, BrowserPartitionId);
        }

    }
}
