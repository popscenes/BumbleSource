using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Flier;

namespace PostaFlya.Domain.Behaviour.Query
{
    public interface FlierBehaviourQueryServiceInterface
    {
        FlierBehaviourInterface GetBehaviourFor(Flier.Flier flier);
    }
}
