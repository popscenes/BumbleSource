using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Azure.Common.TableStorage;
using WebSite.Infrastructure.Domain;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;

namespace PostaFlya.DataRepository.Internal
{
    internal class JsonRepositoryWithBrowser : JsonRepository,
        QueryServiceWithBrowserInterface
    {
        public const int BrowserPartitionId = 1;
        public JsonRepositoryWithBrowser(TableContextInterface tableContext
            , TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService
            , EntityUpdateNotificationInterface<EntityInterface> updateNotification = null)
            : base(tableContext, nameAndPartitionProviderService, updateNotification)
        {
        }

        public IQueryable<EntityType> GetByBrowserId<EntityType>(String browserId) where EntityType : class, BrowserIdInterface, new()
        {
            if (string.IsNullOrWhiteSpace(browserId))
                return null;

            return FindEntitiesByPartition<EntityType>(browserId, BrowserPartitionId);
        }

    }
}
