using System.Collections.Generic;

namespace WebSite.Application.Communication
{
    public interface BroadcastCommunicatorFactoryInterface
    {
        BroadcastCommunicatorInterface GetCommunicatorForEndpoint(string endpoint);
        //IList<BroadcastCommunicatorInterface> Communicators { get; }
    }
}