using WebSite.Application.Command;
using WebSite.Infrastructure.Command;

namespace WebSite.Application.Communication
{
    public interface BroadcastCommunicatorInterface : CommandBusInterface
    {
        string Endpoint { get; }
        QueuedCommandScheduler GetScheduler();
    }
}