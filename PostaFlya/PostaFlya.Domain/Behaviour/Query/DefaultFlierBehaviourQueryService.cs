//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using PostaFlya.Domain.Flier;
//using PostaFlya.Domain.Service;
//using Website.Infrastructure.Query;
////using Website.Infrastructure.Service;
//
//namespace PostaFlya.Domain.Behaviour.Query
//{
//    internal class DefaultFlierBehaviourQueryService : FlierBehaviourQueryServiceInterface
//    {
//        private readonly BehaviourFactoryInterface _behaviourFactory;
//        private readonly GenericQueryServiceInterface _genericQueryService;
//
//        public DefaultFlierBehaviourQueryService(BehaviourFactoryInterface behaviourFactory
//            , GenericQueryServiceInterface genericQueryService)
//        {
//            _behaviourFactory = behaviourFactory;
//            _genericQueryService = genericQueryService;
//        }
//
//        public FlierBehaviourInterface GetBehaviourFor(Flier.Flier flier)
//        {
//            if(flier.FlierBehaviour == FlierBehaviour.Default)
//                return new FlierBehaviourDefault(){Flier = flier};
//            var behaviour = _behaviourFactory.GetDefaultBehaviourTypeForBehaviour(flier.FlierBehaviour);
//            var ret = _genericQueryService.FindById(behaviour, flier.Id) as FlierBehaviourInterface ??
//                      new FlierBehaviourDefault() { Flier = flier };
//            return ret;
//        }
//    }
//}
