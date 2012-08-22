using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Flier;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.Behaviour
{
    [Serializable]
    public abstract class FlierBehaviourBase<BehaviourInterfaceType> 
        : EntityBase<BehaviourInterfaceType>, FlierBehaviourInterface where BehaviourInterfaceType : class, EntityInterface
    {
        public abstract Flier.Flier Flier { get; set; }
        public abstract PropertyGroup FlierProperties { get; set; }
    }
}
