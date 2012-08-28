using System.Collections.Generic;

namespace Website.Application.Communication
{
    public interface BroadcastCommunicatorFactoryInterface
    {
        BroadcastCommunicatorInterface GetCommunicatorForEndpoint(string endpoint);
        //IList<BroadcastCommunicatorInterface> Communicators { get; }
    }
}