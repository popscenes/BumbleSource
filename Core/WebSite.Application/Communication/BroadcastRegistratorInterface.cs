using System.Collections.Generic;

namespace Website.Application.Communication
{
    public interface BroadcastRegistratorInterface
    {
        void RegisterEndpoint(string myEndpoint);
        IList<string> GetCurrentEndpoints();
    }
}