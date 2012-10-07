namespace Website.Application.ApplicationCommunication
{
    public interface BroadcastCommunicatorFactoryInterface
    {
        BroadcastCommunicatorInterface GetCommunicatorForEndpoint(string endpoint);
        //IList<BroadcastCommunicatorInterface> Communicators { get; }
    }
}