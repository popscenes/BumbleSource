using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier.Analytic;
using TechTalk.SpecFlow;
using PostaFlya.Domain.Flier;
using Website.Application.Domain.Browser;
using Website.Domain.Browser;
using Website.Domain.Claims;
using Website.Domain.Comments;
using Website.Domain.Content;
using Website.Domain.Location;
using Website.Domain.Payment;
using Website.Infrastructure.Query;
using Website.Mocks.Domain.Data;
using TestRepositoriesNinjectModule = PostaFlya.Mocks.Domain.Data.TestRepositoriesNinjectModule;


namespace PostaFlya.Specification.Util
{

    public class MockRepositoriesNinjectModule : NinjectModule
    {
        public override void Load()
        {
            var kernel = Kernel as MoqMockingKernel;
            Assert.IsNotNull(kernel, "should be using mock kernel for tests");
            //core
            SetUpBrowserInformation(kernel);
            Website.Mocks.Domain.Data.TestRepositoriesNinjectModule.SetUpWebsiteInfo(kernel);

            Website.Mocks.Domain.Data.TestRepositoriesNinjectModule.SetUpBrowserRepositoryAndQueryService(
                kernel 
                , SpecUtil.GetMockStore<HashSet<BrowserInterface>>("browserstore")
                , SpecUtil.GetMockStore<HashSet<BrowserIdentityProviderCredentialInterface>>("browsercredstore"));

            Website.Mocks.Domain.Data.TestRepositoriesNinjectModule.SetUpImageRepositoryAndQueryService(kernel
                , SpecUtil.GetMockStore<HashSet<ImageInterface>>("imagestore"));

            //postaflya
            TestRepositoriesNinjectModule.SetUpFlierRepositoryAndQueryService(kernel
                , SpecUtil.GetMockStore<HashSet<FlierInterface>>("flierstore")
                , SpecUtil.GetMockStore<HashSet<CommentInterface>>("fliercommentstore")
                , SpecUtil.GetMockStore<HashSet<ClaimInterface>>("claimstore")
                , SpecUtil.GetMockStore<HashSet<BoardFlierInterface>>("boardflierstore")
                , SpecUtil.GetMockStore<HashSet<PaymentTransactionInterface>>("paymentTransactionflierstore")
                , SpecUtil.GetMockStore<HashSet<CreditTransactionInterface>>("creditTransactionflierstore"));

            TestRepositoriesNinjectModule.SetUpBoardRepositoryAndQueryService(kernel
                , SpecUtil.GetMockStore<HashSet<BoardInterface>>("boardstore"));

            TestRepositoriesNinjectModule.SetUpBoardFlierRepositoryAndQueryService(kernel
                , SpecUtil.GetMockStore<HashSet<BoardFlierInterface>>("boardflierstore"));
            
            TestRepositoriesNinjectModule.SetUpAnalyticRepositoryAndQueryService(kernel
                , SpecUtil.GetMockStore<HashSet<FlierAnalyticInterface>>("analyticstore"));


            PrincipalData.SetPrincipal(kernel);

        }

        

        private void SetUpBrowserInformation(MoqMockingKernel kernel)
        {
            
            var mockBrowserInfo = kernel.GetMock<PostaFlyaBrowserInformationInterface>();

            kernel.Bind<BrowserInformationInterface>()
                .ToConstant(mockBrowserInfo.Object).InSingletonScope();

            kernel.Bind<PostaFlyaBrowserInformationInterface>()
                .ToConstant(mockBrowserInfo.Object).InSingletonScope();

            mockBrowserInfo.SetupGet(m => m.Browser).Returns(GetBrowserInfo);
            mockBrowserInfo.SetupSet<BrowserInterface>(m => m.Browser = It.IsAny<BrowserInterface>()).Callback(SetBrowserInfo);
            mockBrowserInfo.SetupGet(bi => bi.IpAddress).Returns("192.168.3.1");
            mockBrowserInfo.SetupGet(bi => bi.UserAgent).Returns("some user agent string");

            Location lastLoc = null;
            mockBrowserInfo.SetupSet<Location>(bi => bi.LastSearchLocation = It.IsAny<Location>())
                           .Callback((loc) 
                               => lastLoc = loc);
            mockBrowserInfo.SetupGet(bi => bi.LastSearchLocation)
                           .Returns(
                           () => lastLoc);

            string trackingId = null;
            mockBrowserInfo.SetupSet<string>(bi => bi.TrackingId = It.IsAny<string>())
                           .Callback((tid) 
                               => trackingId = tid);
            mockBrowserInfo.SetupGet(bi => bi.TrackingId).Returns(
                () 
                    => trackingId);
        }

        public Browser GetBrowserInfo()
        {
            var browser = Kernel.Get<BrowserInterface>(ctx => ctx.Has("defaultbrowser"));
            
            browser.SavedLocations.Add(new Location(20, 20));

            if (ScenarioContext.Current.ContainsKey("browserId"))
            {
                browser = Kernel.Get<GenericQueryServiceInterface>()
                    .FindById<Browser>(ScenarioContext.Current.Get<string>("browserId"));
            }
            else
            {
                SetBrowserInfo(browser);
            }

            return browser as Browser;
        }

        public static void SetBrowserInfo(BrowserInterface newBrowser)
        {
                if (ScenarioContext.Current.ContainsKey("browserId"))
                {
                    ScenarioContext.Current.Remove("browserId");
                    ScenarioContext.Current.Add("browserId", newBrowser.Id);

                }
                else
                {
                    ScenarioContext.Current.Add("browserId", newBrowser.Id);
                }
            }
    }


}


