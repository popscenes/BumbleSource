using System;
using System.Collections.Generic;
using Ninject;
using Ninject.Syntax;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Service;

namespace PostaFlya.Domain.Behaviour
{
    public class BehaviourFactory : BehaviourFactoryInterface
    {
        private readonly IResolutionRoot _resolutionRoot;
        public BehaviourFactory(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }

//        public BehaviourType GetDefaultBehaviourInstanceForBehaviour<BehaviourType>(FlierBehaviour behaviour) where BehaviourType : class, FlierBehaviourInterface
//        {
//            var ret = _resolutionRoot.Get<FlierBehaviourInterface>(ctx => ctx.Get<FlierBehaviour>("flierbehaviour") == behaviour);
//            return ret as BehaviourType;
//        }

        public Type GetDefaultBehaviourTypeForBehaviour(FlierBehaviour behaviour)
        {
            var ret = _resolutionRoot.Get<Dictionary<FlierBehaviour, Type>>(ctx => ctx.Has("flierbehaviour"));
            return ret.ContainsKey(behaviour) ?  ret[behaviour] : typeof (FlierBehaviourDefault);
        }

        public FlierBehaviourInterface CreateBehaviourInstanceForFlier(Flier.Flier flier)
        {
            var flierBehaviour = _resolutionRoot.Get(GetDefaultBehaviourTypeForBehaviour(flier.FlierBehaviour)) as FlierBehaviourInterface;
            if (flierBehaviour == null)
                return null;

            flierBehaviour.Flier = flier;
            return flierBehaviour;
        }
    }
}
