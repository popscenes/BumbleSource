using System;
using System.Runtime.Caching;
using Website.Application.Binding;
using Website.Infrastructure.Caching;
using Website.Infrastructure.Caching.Command;
using Website.Infrastructure.Command;

namespace Website.Application.Caching.Command
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
