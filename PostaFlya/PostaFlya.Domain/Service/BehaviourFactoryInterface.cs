using System;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;

namespace PostaFlya.Domain.Service
{
    public interface BehaviourFactoryInterface
    {
        Type GetDefaultBehaviourTypeForBehaviour(FlierBehaviour behaviour);
        FlierBehaviourInterface CreateBehaviourInstanceForFlier(Flier.Flier flier);
    }
}
