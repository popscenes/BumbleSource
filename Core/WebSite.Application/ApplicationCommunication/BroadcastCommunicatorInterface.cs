using Website.Application.Command;
using Website.Infrastructure.Command;

namespace Website.Application.ApplicationCommunication
{
    public interface BroadcastCommunicatorInterface : CommandBusInterface
    {
        string Endpoint { get; }
        QueuedCommandScheduler GetScheduler();
    }
}