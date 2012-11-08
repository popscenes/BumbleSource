using Website.Infrastructure.Publish;

namespace Website.Domain.Browser.Publish
{
    public interface BrowserSubscriptionInterface : SubscriptionInterface
    {
        bool IsBrowserSubscribed(BrowserInterface browser);
        bool BrowserSubscribe(BrowserInterface browser);
        bool BrowserUnsubscribe(BrowserInterface browser);
    }

    public interface BrowserSubscriptionInterface<in PublishType> 
        : SubscriptionInterface<PublishType>, BrowserSubscriptionInterface
    {

    }
}
