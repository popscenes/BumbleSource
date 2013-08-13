using Website.Infrastructure.Messaging;
using Website.Infrastructure.Publish;

namespace Website.Domain.Browser.Publish
{
    public interface BrowserSubscriptionInterface : HandleEventInterface
    {
        bool IsBrowserSubscribed(Website.Domain.Browser.BrowserInterface browser);
        bool BrowserSubscribe(Website.Domain.Browser.BrowserInterface browser);
        bool BrowserUnsubscribe(Website.Domain.Browser.BrowserInterface browser);
    }

    public interface BrowserSubscriptionInterface<in PublishType> 
        : HandleEventInterface<PublishType>, BrowserSubscriptionInterface
    {
        string SubscriptionName { get; }
    }
}
