using System.Collections.Generic;

namespace Website.Application.ApplicationCommunication
{
    public interface ApplicationBroadcastCommunicatorRegistrationInterface
    {
        void RegisterEndpoint(string myEndpoint);
        IList<string> GetCurrentEndpoints();
    }
}