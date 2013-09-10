using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website.Infrastructure.Domain
{
    public interface EntityModifiedEventInterface<EntityType> : EventInterface
    {
        EntityType Entity { get; set; }
        bool IsDeleted { get; set; }
    }
}
