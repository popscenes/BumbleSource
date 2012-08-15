using WebSite.Infrastructure.Command;

namespace WebSite.Application.Command
{
    public interface CommandQueueFactoryInterface
    {
        CommandBusInterface GetCommandBusForEndpoint(string queueEndpoint);
        void Delete(string queueEndpoint);
        QueuedCommandScheduler GetSchedulerForEndpoint(string queueEndpoint);
    }
}