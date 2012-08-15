using System.Collections.Generic;

namespace WebSite.Azure.Common.TableStorage
{
    public interface EntityUpdateNotificationInterface<in DomainEntityInterfaceType>
    {
        void NotifyUpdate(IEnumerable<DomainEntityInterfaceType> values);
        void NotifyDelete(IEnumerable<DomainEntityInterfaceType> values);
    }
}