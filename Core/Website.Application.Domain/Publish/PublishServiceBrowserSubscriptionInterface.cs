using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Application.Publish;
using Website.Domain.Browser;

namespace Website.Application.Domain.Publish
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
