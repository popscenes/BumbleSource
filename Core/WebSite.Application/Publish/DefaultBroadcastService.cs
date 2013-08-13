using System;
using System.Linq;
using System.Threading.Tasks;
using Ninject;
using Ninject.Syntax;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Publish;

namespace Website.Application.Publish
{
    public class DefaultBroadcastService : BroadcastServiceInterface
    {
        private readonly IResolutionRoot _resolutionRoot;
        private readonly Type _genericEventHandlerTyp = typeof(HandleEventInterface<>);

        public DefaultBroadcastService(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }
        readonly ParallelOptions _parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 5 };
        public bool Broadcast(object broadcastObject)
        {
            var eventHandlerGen = _genericEventHandlerTyp.MakeGenericType(broadcastObject.GetType());
            var pubservices = _resolutionRoot.GetAll(eventHandlerGen).Cast<dynamic>();

            dynamic ob = broadcastObject;

            var res = Parallel.ForEach(pubservices, _parallelOptions,
                                       o =>
                                       o.Handle(ob));

            return res.IsCompleted;
        }
    }
}