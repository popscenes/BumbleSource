using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website.Infrastructure.Domain
{
    public interface EntityModifiedEventInterface<EntityType> : EventInterface
        where EntityType : EntityInterface
    {
        EntityType OrigState { get; set; }
        EntityType NewState { get; set; }
    }
}
