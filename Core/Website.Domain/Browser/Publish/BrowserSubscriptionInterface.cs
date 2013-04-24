using Website.Infrastructure.Publish;

namespace Website.Domain.Browser.Publish
{
    public interface BrowserSubscriptionInterface : HandleEventInterface
    {
        bool IsBrowserSubscribed(BrowserInterface browser);
        bool BrowserSubscribe(BrowserInterface browser);
        bool BrowserUnsubscribe(BrowserInterface browser);
    }

    public interface BrowserSubscriptionInterface<in PublishType> 
        : HandleEventInterface<PublishType>, BrowserSubscriptionInterface
    {
        string SubscriptionName { get; }
    }
}
