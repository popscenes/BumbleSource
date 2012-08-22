using System;
using System.Runtime.Caching;
using WebSite.Application.Binding;
using WebSite.Infrastructure.Caching;
using WebSite.Infrastructure.Caching.Command;
using WebSite.Infrastructure.Command;

namespace WebSite.Application.Caching.Command
{
    public class CachedNotificationBusAttribute : Attribute {}
    public class CacheNotifier
    {
        private readonly CommandBusInterface _notificationCommandBus;

        public CacheNotifier([CachedNotificationBus]CommandBusInterface notificationCommandBus = null,
            bool enableNotifications = true)
        {
            _notificationCommandBus = notificationCommandBus;
            if (!enableNotifications)
                _notificationCommandBus = null;
        }

        public void NotifyInvalidateKey(string region, string key)
        {
            if (_notificationCommandBus != null)
            {
                _notificationCommandBus.Send(
                    new InvalidateCacheDataCommand()
                    {
                        Key = key,
                        Region = region,
                        CommandId = Guid.NewGuid().ToString()
                    }
                    );
            }
        }
    }

    public class BroadcastCachedRepository : CachedRepositoryBase
    {
        private readonly CacheNotifier _notifier;

        protected BroadcastCachedRepository(ObjectCache cacheProvider
            , string regionName
            , CacheNotifier notifier
            , GenericRepositoryInterface genericRepository)
            : base(cacheProvider, regionName, genericRepository)
        {
            _notifier = notifier;
        }

        protected override void InvalidateCachedData(string cacheKey)
        {
            base.InvalidateCachedData(cacheKey);
            _notifier.NotifyInvalidateKey(GetRegion(), GetInternalKey(cacheKey));
        }
    }
}
