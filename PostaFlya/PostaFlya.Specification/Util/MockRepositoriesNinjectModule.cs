using System;
using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using TechTalk.SpecFlow;
using Website.Application.Authentication;
using Website.Application.WebsiteInformation;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.TaskJob;
using Website.Infrastructure.Authentication;
using Website.Application.Domain.Browser;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;
using Website.Domain.Claims;
using Website.Domain.Comments;
using Website.Domain.Content;
using Website.Domain.Location;
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

            SetUpBrowserInformation(kernel);
            Website.Mocks.Domain.Data.TestRepositoriesNinjectModule.SetUpWebsiteInfo(kernel);

            Website.Mocks.Domain.Data.TestRepositoriesNinjectModule.SetUpBrowserRepositoryAndQueryService(kernel, SpecUtil.GetMockStore<HashSet<BrowserInterface>>("browserstore"));
            TestRepositoriesNinjectModule.SetUpFlierRepositoryAndQueryService(kernel
                , SpecUtil.GetMockStore<HashSet<FlierInterface>>("flierstore")
                , SpecUtil.GetMockStore<HashSet<CommentInterface>>("fliercommentstore")
                , SpecUtil.GetMockStore<HashSet<ClaimInterface>>("claimstore"));
            Website.Mocks.Domain.Data.TestRepositoriesNinjectModule.SetUpImageRepositoryAndQueryService(kernel
                , SpecUtil.GetMockStore<HashSet<ImageInterface>>("imagestore"));

            

            PrincipalData.SetPrincipal(kernel);

            //behaviours
            TestRepositoriesNinjectModule.SetUpTaskJobRepositoryAndQueryService(kernel 
                ,SpecUtil.GetMockStore<HashSet<TaskJobFlierBehaviourInterface>>("taskjobstore")
                ,SpecUtil.GetMockStore<HashSet<TaskJobBidInterface>>("taskjobbidstore"));
        }

        

        private void SetUpBrowserInformation(MoqMockingKernel kernel)
        {
            var mockBrowserInfo = kernel.GetMock<BrowserInformationInterface>();
            kernel.Bind<BrowserInformationInterface>()
                .ToConstant(mockBrowserInfo.Object).InSingletonScope();

            mockBrowserInfo.SetupGet(m => m.Browser).Returns(GetBrowserInfo);
            mockBrowserInfo.SetupSet<BrowserInterface>(m => m.Browser = It.IsAny<BrowserInterface>()).Callback(SetBrowserInfo);
        }

        public Browser GetBrowserInfo()
        {
            var browser = Kernel.Get<BrowserInterface>(ctx => ctx.Has("defaultbrowser"));
            
            browser.SavedLocations.Add(new Location(20, 20));

            if (ScenarioContext.Current.ContainsKey("browserId"))
            {
                browser = Kernel.Get<BrowserQueryServiceInterface>()
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


