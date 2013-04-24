using System.Linq;
using Website.Domain.Browser.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Util.Extension;

namespace Website.Domain.Browser.Publish
{
    public abstract class BrowserSubscriptionBase<PublishType> : BrowserSubscriptionInterface<PublishType>
    {
        private readonly CommandBusInterface _commandBus;

        
        protected BrowserSubscriptionBase(CommandBusInterface commandBus)
        {
            _commandBus = commandBus;
        }

        public bool Handle(PublishType @event)
        {
            var browsers = GetBrowsersForPublish(@event);
            if (browsers == null || browsers.Length == 0)
                return false;
            return browsers.Where(IsBrowserSubscribed).Aggregate(false, 
                                (current, brows) => 
                                    PublishToBrowser(brows, @event) || current);
        }

        public bool IsBrowserSubscribed(BrowserInterface browser)
        {
            const bool ret = true;
            return browser.Properties.GetOrDefault(SubscriptionName, ret);
        }

        public bool BrowserSubscribe(BrowserInterface browser)
        {
            _commandBus.Send(new SetBrowserPropertyCommand()
                                 {
                                     Browser = browser,
                                     PropertyName = SubscriptionName,
                                     PropertyValue = true
                                 });
            return true;
        }

        public bool BrowserUnsubscribe(BrowserInterface browser)
        {
            _commandBus.Send(new SetBrowserPropertyCommand()
                                 {
                                     Browser = browser,
                                     PropertyName = SubscriptionName,
                                     PropertyValue = false
                                 });
            return IsBrowserSubscribed(browser);
        }

        public abstract BrowserInterface[] GetBrowsersForPublish(PublishType publish);
        public abstract bool PublishToBrowser(BrowserInterface browser, PublishType publish);
        public abstract string SubscriptionName { get; }
    }
}