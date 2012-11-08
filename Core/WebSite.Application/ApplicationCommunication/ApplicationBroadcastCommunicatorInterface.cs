using Website.Application.Command;
using Website.Infrastructure.Command;

namespace Website.Application.ApplicationCommunication
{
    public interface ApplicationBroadcastCommunicatorInterface : CommandBusInterface
    {
        string Endpoint { get; }
        QueuedCommandProcessor GetScheduler();
    }
}