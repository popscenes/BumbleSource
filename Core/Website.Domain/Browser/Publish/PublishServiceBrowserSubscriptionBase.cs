using System.Linq;
using Website.Domain.Browser.Command;
using Website.Infrastructure.Command;
using Website.Infrastructure.Util.Extension;

namespace Website.Domain.Browser.Publish
{
    public abstract class PublishServiceBrowserSubscriptionBase<PublishType> : PublishServiceBrowserSubscriptionInterface<PublishType>
    {
        private readonly CommandBusInterface _commandBus;

        protected PublishServiceBrowserSubscriptionBase(CommandBusInterface commandBus)
        {
            _commandBus = commandBus;
        }

        public abstract string Name { get; }
        public abstract string Description { get; }           
        public bool Publish(PublishType publish)
        {
            var browsers = GetBrowsersForPublish(publish);
            return browsers.Where(IsBrowserSubscribed).Aggregate(false, 
                                (current, brows) => 
                                    PublishToBrowser(brows, publish) || current);
        }

        public bool IsEnabled { get { return true; } }

        public bool IsBrowserSubscribed(BrowserInterface browser)
        {
            const bool ret = false;
            return browser.Properties.GetOrDefault(Name, ret);
        }

        public bool BrowserSubscribe(BrowserInterface browser)
        {
            _commandBus.Send(new SetBrowserPropertyCommand()
                                 {
                                     Browser = browser,
                                     PropertyName = Name,
                                     PropertyValue = true
                                 });
            return IsBrowserSubscribed(browser);
        }

        public bool BrowserUnsubscribe(BrowserInterface browser)
        {
            _commandBus.Send(new SetBrowserPropertyCommand()
                                 {
                                     Browser = browser,
                                     PropertyName = Name,
                                     PropertyValue = false
                                 });
            return IsBrowserSubscribed(browser);
        }

        public abstract BrowserInterface[] GetBrowsersForPublish(PublishType publish);
        public abstract bool PublishToBrowser(BrowserInterface browser, PublishType publish);
    }
}