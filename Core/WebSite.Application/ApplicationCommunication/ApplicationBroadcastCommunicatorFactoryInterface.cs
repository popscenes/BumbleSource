namespace Website.Application.ApplicationCommunication
{
    public interface ApplicationBroadcastCommunicatorFactoryInterface
    {
        ApplicationBroadcastCommunicatorInterface GetCommunicatorForEndpoint(string endpoint);
        //IList<BroadcastCommunicatorInterface> Communicators { get; }
    }
}