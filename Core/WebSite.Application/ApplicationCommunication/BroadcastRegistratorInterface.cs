using System.Collections.Generic;

namespace Website.Application.ApplicationCommunication
{
    public interface BroadcastRegistratorInterface
    {
        void RegisterEndpoint(string myEndpoint);
        IList<string> GetCurrentEndpoints();
    }
}