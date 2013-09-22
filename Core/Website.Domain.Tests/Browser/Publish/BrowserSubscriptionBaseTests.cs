using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Publish;
using Website.Domain.Browser;
using Website.Domain.Browser.Command;
using Website.Domain.Browser.Publish;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Publish;
using Website.Infrastructure.Query;
using Website.Mocks.Domain.Data;

namespace Website.Domain.Tests.Browser.Publish
{
    [TestFixture]
    public class BrowserSubscriptionBaseTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        public static void FixtureSetUp(MoqMockingKernel kernel)
        {

            kernel.Bind<MessageBusInterface>().To<InMemoryMessageBus>();
            kernel.Bind<BrowserSubscriptionInterface>().To<TestPublishClass>();
            kernel.Bind<HandleEventInterface<TestPublishObject>>().To<TestPublishClass>();
            kernel.Bind<BroadcastServiceInterface>().To<DefaultBroadcastService>();
            // kernel.Bind<MessageHandlerInterface<SetBrowserPropertyCommand>>().To<SetBrowserPropertyCommandHandler>();

            kernel.Bind<MessageHandlerInterface<SetBrowserPropertyCommand>>().To<TestSetBrowserPropertyCommandHandler>();
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            FixtureSetUp(Kernel);
        }

        public static void FixtureTearDown(MoqMockingKernel kernel)
        {
            kernel.Unbind<MessageBusInterface>();
            kernel.Unbind<BrowserSubscriptionInterface>();
            kernel.Unbind<HandleEventInterface<TestPublishObject>>();
            kernel.Unbind<BroadcastServiceInterface>();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            FixtureTearDown(Kernel);
        }

        [Test]
        public void PublishServiceBrowserSubscriptionBaseHandlesBrowsesrSubscription()
        {
            var repo = Kernel.Get<GenericRepositoryInterface>();
            var browser = BrowserTestData.GetOne(Kernel);
            BrowserTestData.StoreOne(browser, repo, Kernel);
            var qs = Kernel.Get<GenericQueryServiceInterface>();

            var testSub = Kernel.Get<BrowserSubscriptionInterface>();
            Assert.That(testSub, Is.InstanceOf<TestPublishClass>());

            Assert.IsTrue(testSub.IsBrowserSubscribed(browser));

            testSub.BrowserUnsubscribe(browser);

            browser = qs.FindById<Website.Domain.Browser.Browser>(browser.Id);

            Assert.IsFalse(testSub.IsBrowserSubscribed(browser));

            testSub.BrowserSubscribe(browser);

            browser = BrowserTestData.AssertGetById(browser, qs);

            Assert.IsTrue(testSub.IsBrowserSubscribed(browser));

            testSub.BrowserUnsubscribe(browser);

            browser = BrowserTestData.AssertGetById(browser, Kernel.Get<GenericQueryServiceInterface>());

            Assert.IsFalse(testSub.IsBrowserSubscribed(browser));
        }

        [Test]
        public void PublishServiceBrowserSubscriptionBasePublishesToAllBrowsersRequiredForPublish()
        {
            var repo = Kernel.Get<GenericRepositoryInterface>();
            var browser = BrowserTestData.GetOne(Kernel);
            BrowserTestData.StoreOne(browser, repo, Kernel);

            var browserTwo = BrowserTestData.GetOne(Kernel);
            BrowserTestData.StoreOne(browserTwo, repo, Kernel);

            var testSub = Kernel.Get<BrowserSubscriptionInterface>();
            Assert.That(testSub, Is.InstanceOf<TestPublishClass>());


            testSub.BrowserSubscribe(browser);
            testSub.BrowserSubscribe(browserTwo);

            browser = BrowserTestData.AssertGetById(browser, Kernel.Get<GenericQueryServiceInterface>());
            browserTwo = BrowserTestData.AssertGetById(browserTwo, Kernel.Get<GenericQueryServiceInterface>());

            Assert.IsTrue(testSub.IsBrowserSubscribed(browser));
            Assert.IsTrue(testSub.IsBrowserSubscribed(browserTwo));

            _publishedBrowser.Clear();
            var broadcastService = Kernel.Get<BroadcastServiceInterface>();
            var ret = broadcastService.Broadcast(new TestPublishObject()
                                                     {
                                                         BrowserIds = new[] {browser.Id, browserTwo.Id}
                                                     });
            Assert.IsNotNull(ret);
            Assert.IsTrue((bool) ret);
            Assert.That(_publishedBrowser.Count(), Is.EqualTo(2));
            _publishedBrowser.Clear();
        }

        private static readonly List<BrowserInterface> _publishedBrowser = new List<BrowserInterface>(); 
        public class TestPublishObject
        {
            public string[] BrowserIds { get; set; }

        }
        public class TestPublishClass : BrowserSubscriptionBase<TestPublishObject>
        {
            private readonly GenericQueryServiceInterface _browserQueryService;
            public TestPublishClass(MessageBusInterface messageBus, GenericQueryServiceInterface browserQueryService) 
                : base(messageBus)
            {
                _browserQueryService = browserQueryService;
            }

            public override string SubscriptionName
            {
                get { return "TestPublish"; }
            }
            public override BrowserInterface[] GetBrowsersForPublish(TestPublishObject publish)
            {
                return publish.BrowserIds.Aggregate(new List<BrowserInterface>(), 
                    (list, id) =>
                        {
                            list.Add(_browserQueryService.FindById<Website.Domain.Browser.Browser>(id));
                            return list;
                        })
                        .Where(b => b != null)
                        .ToArray();
            }

            public override bool PublishToBrowser(BrowserInterface browser, TestPublishObject publish)
            {
                _publishedBrowser.Add(browser);
                return true;
            }
        }
    }

    internal class TestSetBrowserPropertyCommandHandler : MessageHandlerInterface<SetBrowserPropertyCommand>
    {
        private readonly GenericRepositoryInterface _repository;

        public TestSetBrowserPropertyCommandHandler(GenericRepositoryInterface repository)
        {
            _repository = repository;
        }

        public void Handle(SetBrowserPropertyCommand command)
        {
            _repository.UpdateEntity<Website.Domain.Browser.Browser>(
                command.Browser.Id,
                browser =>
                browser.Properties[command.PropertyName] = command.PropertyValue);
            
        }
    }
}
