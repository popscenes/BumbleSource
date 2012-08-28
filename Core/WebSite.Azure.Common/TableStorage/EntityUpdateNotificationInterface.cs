using System.Collections.Generic;

namespace Website.Azure.Common.TableStorage
{
    public interface EntityUpdateNotificationInterface<in DomainEntityInterfaceType>
    {
        void NotifyUpdate(IEnumerable<DomainEntityInterfaceType> values);
        void NotifyDelete(IEnumerable<DomainEntityInterfaceType> values);
    }
}