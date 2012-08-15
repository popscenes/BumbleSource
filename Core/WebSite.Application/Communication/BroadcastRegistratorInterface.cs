using System.Collections.Generic;

namespace WebSite.Application.Communication
{
    public interface BroadcastRegistratorInterface
    {
        void RegisterEndpoint(string myEndpoint);
        IList<string> GetCurrentEndpoints();
    }
}