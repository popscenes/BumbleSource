using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Domain.Browser;
using Website.Domain.Browser.Command;
using Website.Domain.Browser.Publish;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Command;
using Website.Infrastructure.Publish;
using Website.Infrastructure.Service;
using Website.Mocks.Domain.Data;

namespace Website.Domain.Tests.Browser.Publish
{
    [TestFixture]
    public class PublishServiceBrowserSubscriptionBaseTests
    {
        MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        public static void FixtureSetUp(MoqMockingKernel kernel)
        {

            kernel.Bind<CommandBusInterface>().To<DefaultCommandBus>();
            kernel.Bind<PublishServiceBrowserSubscriptionInterface>().To<TestPublishClass>();
            kernel.Bind<PublishServiceInterface<TestPublishObject>>().To<TestPublishClass>();
           // kernel.Bind<CommandHandlerInterface<SetBrowserPropertyCommand>>().To<SetBrowserPropertyCommandHandler>();

            
        }

        [FixtureSetUp]
        public void FixtureSetUp()
        {
            FixtureSetUp(Kernel);
        }

        public static void FixtureTearDown(MoqMockingKernel kernel)
        {
            kernel.Unbind<CommandBusInterface>();
            kernel.Unbind<PublishServiceBrowserSubscriptionInterface>();
            kernel.Unbind<PublishServiceInterface<TestPublishObject>>();
        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {
            FixtureTearDown(Kernel);
        }

        [Test]
        public void PublishServiceBrowserSubscriptionBaseHandlesBrowsesrSubscription()
        {
            var repo = Kernel.Get<BrowserRepositoryInterface>();
            var browser = BrowserTestData.GetOne(Kernel);
            BrowserTestData.StoreOne(browser, repo, Kernel);

            var testSub = Kernel.Get<PublishServiceBrowserSubscriptionInterface>();
            Assert.IsInstanceOfType<TestPublishClass>(testSub);

            Assert.IsFalse(testSub.IsBrowserSubscribed(browser));

            testSub.BrowserSubscribe(browser);

            browser = BrowserTestData.AssertGetById(browser, Kernel.Get<BrowserQueryServiceInterface>());

            Assert.IsTrue(testSub.IsBrowserSubscribed(browser));

            testSub.BrowserUnsubscribe(browser);

            browser = BrowserTestData.AssertGetById(browser, Kernel.Get<BrowserQueryServiceInterface>());

            Assert.IsFalse(testSub.IsBrowserSubscribed(browser));
        }

        [Test]
        public void PublishServiceBrowserSubscriptionBasePublishesToAllBrowsersRequiredForPublish()
        {
            var repo = Kernel.Get<BrowserRepositoryInterface>();
            var browser = BrowserTestData.GetOne(Kernel);
            BrowserTestData.StoreOne(browser, repo, Kernel);

            var browserTwo = BrowserTestData.GetOne(Kernel);
            BrowserTestData.StoreOne(browserTwo, repo, Kernel);

            var testSub = Kernel.Get<PublishServiceBrowserSubscriptionInterface>();
            Assert.IsInstanceOfType<TestPublishClass>(testSub);

            Assert.IsFalse(testSub.IsBrowserSubscribed(browser));

            testSub.BrowserSubscribe(browser);
            testSub.BrowserSubscribe(browserTwo);

            browser = BrowserTestData.AssertGetById(browser, Kernel.Get<BrowserQueryServiceInterface>());
            browserTwo = BrowserTestData.AssertGetById(browserTwo, Kernel.Get<BrowserQueryServiceInterface>());

            Assert.IsTrue(testSub.IsBrowserSubscribed(browser));
            Assert.IsTrue(testSub.IsBrowserSubscribed(browserTwo));

            _publishedBrowser.Clear();
            var broadcastService = Kernel.Get<PublishBroadcastServiceInterface>();
            var ret = broadcastService.Broadcast(new TestPublishObject()
                                                     {
                                                         BrowserIds = new[] {browser.Id, browserTwo.Id}
                                                     });
            Assert.IsNotNull(ret);
            Assert.IsTrue((bool) ret);
            Assert.Count(2, _publishedBrowser);
            _publishedBrowser.Clear();
        }

        private static readonly List<BrowserInterface> _publishedBrowser = new List<BrowserInterface>(); 
        public class TestPublishObject
        {
            public string[] BrowserIds { get; set; }

        }
        public class TestPublishClass : PublishServiceBrowserSubscriptionBase<TestPublishObject>
        {
            private readonly BrowserQueryServiceInterface _browserQueryService;
            public TestPublishClass(CommandBusInterface commandBus, BrowserQueryServiceInterface browserQueryService) : base(commandBus)
            {
                _browserQueryService = browserQueryService;
            }

            public override string Name
            {
                get { return "TestPublish"; }
            }

            public override string Description
            {
                get { return "Test Publish Service"; }
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
}
