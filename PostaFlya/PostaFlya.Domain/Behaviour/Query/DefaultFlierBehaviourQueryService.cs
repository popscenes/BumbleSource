using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Service;
using WebSite.Infrastructure.Query;
using WebSite.Infrastructure.Service;

namespace PostaFlya.Domain.Behaviour.Query
{
    internal class DefaultFlierBehaviourQueryService : FlierBehaviourQueryServiceInterface
    {
        private readonly BehaviourFactoryInterface _behaviourFactory;
        private readonly GenericServiceFactoryInterface _genericServiceFactory;

        public DefaultFlierBehaviourQueryService(BehaviourFactoryInterface behaviourFactory
            , GenericServiceFactoryInterface genericServiceFactory)
        {
            _behaviourFactory = behaviourFactory;
            _genericServiceFactory = genericServiceFactory;
        }

        public FlierBehaviourInterface GetBehaviourFor(FlierInterface flier)
        {
            if(flier.FlierBehaviour == FlierBehaviour.Default)
                return new FlierBehaviourDefault(){Flier = flier};
            var behaviour = _behaviourFactory.GetDefaultBehaviourTypeForBehaviour(flier.FlierBehaviour);
            var queryService = _genericServiceFactory.GetGenericQueryService<QueryServiceInterface>(behaviour);
            var ret = queryService.FindById(flier.Id) as FlierBehaviourInterface ??
                      new FlierBehaviourDefault() { Flier = flier };
            return ret;
        }
    }
}
