using Website.Application.Command;
using Website.Infrastructure.Command;

namespace Website.Application.Communication
{
    public interface BroadcastCommunicatorInterface : CommandBusInterface
    {
        string Endpoint { get; }
        QueuedCommandScheduler GetScheduler();
    }
}