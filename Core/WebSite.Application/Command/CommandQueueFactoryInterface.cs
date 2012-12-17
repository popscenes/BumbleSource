using Website.Infrastructure.Command;

namespace Website.Application.Command
{
    public interface CommandQueueFactoryInterface 
    {
        CommandBusInterface GetCommandBusForEndpoint(string queueEndpoint);
        void Delete(string queueEndpoint);
        QueuedCommandProcessor GetSchedulerForEndpoint(string queueEndpoint);
    }
}