using System;
using System.Linq;
using System.Threading.Tasks;
using Ninject;
using Ninject.Syntax;
using Website.Infrastructure.Service;

namespace Website.Infrastructure.Publish
{
    public class DefaultPublishBroadcastService : PublishBroadcastServiceInterface
    {
        private readonly IResolutionRoot _resolutionRoot;
        private readonly Type _publishService = typeof(PublishServiceInterface<>);

        public DefaultPublishBroadcastService(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }
        readonly ParallelOptions _parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 5 };
        public bool Broadcast(object broadcastObject)
        {
            var publishService = _publishService.MakeGenericType(broadcastObject.GetType());
            var pubservices = _resolutionRoot.GetAll(publishService)
                .Cast<dynamic>()
                .Where(ps => ps.IsEnabled);
            dynamic ob = broadcastObject;

            var res = Parallel.ForEach(pubservices, _parallelOptions,
                                       o =>
                                       o.Publish(ob));

            return res.IsCompleted;
        }
    }
}