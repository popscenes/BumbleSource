using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Application.Command;
using Website.Infrastructure.Command;

namespace Website.Application.ApplicationCommunication
{
    public class DefaultApplicationBroadcastCommunicator : ApplicationBroadcastCommunicatorInterface
    {
        private readonly string _myEndpoint;
        private readonly CommandQueueFactoryInterface _commandQueueFactory;
        private readonly ApplicationBroadcastCommunicatorRegistrationInterface _applicationBroadcastCommunicatorRegistration;

        private readonly ConcurrentDictionary<string, CommandBusInterface> _commandBusses = new ConcurrentDictionary<string, CommandBusInterface>();

        public DefaultApplicationBroadcastCommunicator(string myEndpoint
                                            , CommandQueueFactoryInterface commandQueueFactory
                                            , ApplicationBroadcastCommunicatorRegistrationInterface applicationBroadcastCommunicatorRegistration)
        {
            _myEndpoint = myEndpoint;
            _commandQueueFactory = commandQueueFactory;
            _applicationBroadcastCommunicatorRegistration = applicationBroadcastCommunicatorRegistration;
            Register();
        }

        internal void Register()
        {
            _applicationBroadcastCommunicatorRegistration.RegisterEndpoint(_myEndpoint);
            IList<string> enpoints =  _applicationBroadcastCommunicatorRegistration.GetCurrentEndpoints();
            _commandBusses.Clear();
            foreach (var enpoint in enpoints
                .Select(e => new { EndPoint = e, CommandQueue = _commandQueueFactory.GetCommandBusForEndpoint(e)}))
            {
                if (enpoint.EndPoint != _myEndpoint)
                    _commandBusses[enpoint.EndPoint] = enpoint.CommandQueue;
            }
        }

        readonly ParallelOptions _parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 5 };
        public object Send<CommandType>(CommandType command) where CommandType : class, CommandInterface
        {
            return Parallel.ForEach(_commandBusses, _parallelOptions, kv => kv.Value.Send(command));
        }

        public string Endpoint
        {
            get { return _myEndpoint; }
        }

        public QueuedCommandProcessor GetScheduler()
        {
            return _commandQueueFactory.GetSchedulerForEndpoint(Endpoint);
        }
    }
}