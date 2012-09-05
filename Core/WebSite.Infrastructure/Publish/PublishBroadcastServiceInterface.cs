namespace Website.Infrastructure.Publish
{
    public interface PublishBroadcastServiceInterface
    {
        bool Broadcast(object broadcastObject);
    }
}