using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSite.Application.Command;
using WebSite.Infrastructure.Command;

namespace WebSite.Application.Communication
{
    public class DefaultBroadcastCommunicator : BroadcastCommunicatorInterface
    {
        private readonly string _myEndpoint;
        private readonly CommandQueueFactoryInterface _commandQueueFactory;
        private readonly BroadcastRegistratorInterface _broadcastRegistrator;

        private readonly ConcurrentDictionary<string, CommandBusInterface> _commandBusses = new ConcurrentDictionary<string, CommandBusInterface>();

        public DefaultBroadcastCommunicator(string myEndpoint
                                            , CommandQueueFactoryInterface commandQueueFactory
                                            , BroadcastRegistratorInterface broadcastRegistrator)
        {
            _myEndpoint = myEndpoint;
            _commandQueueFactory = commandQueueFactory;
            _broadcastRegistrator = broadcastRegistrator;
            Register();
        }

        internal void Register()
        {
            _broadcastRegistrator.RegisterEndpoint(_myEndpoint);
            IList<string> enpoints =  _broadcastRegistrator.GetCurrentEndpoints();
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

        public QueuedCommandScheduler GetScheduler()
        {
            return _commandQueueFactory.GetSchedulerForEndpoint(Endpoint);
        }
    }
}