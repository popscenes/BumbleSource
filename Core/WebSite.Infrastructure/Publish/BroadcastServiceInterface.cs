namespace Website.Infrastructure.Publish
{
    public interface BroadcastServiceInterface
    {
        bool Broadcast(object broadcastObject);
    }
}