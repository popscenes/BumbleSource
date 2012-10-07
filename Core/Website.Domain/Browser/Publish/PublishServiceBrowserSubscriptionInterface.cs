using Website.Infrastructure.Publish;

namespace Website.Domain.Browser.Publish
{
    public interface PublishServiceBrowserSubscriptionInterface : PublishServiceInterface
    {
        bool IsBrowserSubscribed(BrowserInterface browser);
        bool BrowserSubscribe(BrowserInterface browser);
        bool BrowserUnsubscribe(BrowserInterface browser);
    }

    public interface PublishServiceBrowserSubscriptionInterface<in PublishType> 
        : PublishServiceInterface<PublishType>, PublishServiceBrowserSubscriptionInterface
    {

    }
}
