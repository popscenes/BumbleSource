using System;
using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using WebSite.Infrastructure.Authentication;
using WebSite.Application.WebsiteInformation;
using Website.Domain.Browser;
using Website.Domain.Browser.Command;
using Website.Domain.Browser.Query;
using Website.Domain.Comments;
using Website.Domain.Content;
using Website.Domain.Content.Command;
using Website.Domain.Content.Query;
using Website.Domain.Likes;
using Website.Domain.Location;
using Website.Domain.Tag;

namespace Website.Mocks.Domain.Data
{

    public class TestRepositoriesNinjectModule : NinjectModule
    {
        public override void Load()
        {
            var kernel = Kernel as MoqMockingKernel;
            Assert.IsNotNull(kernel, "should be using mock kernel for tests");
            SetUpBrowserRepositoryAndQueryService(kernel, RepoUtil.GetMockStore<BrowserInterface>());
            SetUpImageRepositoryAndQueryService(kernel, RepoUtil.GetMockStore<ImageInterface>());   
            PrincipalData.SetPrincipal(kernel);
            SetUpWebsiteInfo(kernel);

        }

        public static void SetUpImageRepositoryAndQueryService(MoqMockingKernel kernel, HashSet<ImageInterface> store)
        {
            //repo
            var imageRepository = RepoUtil.SetupRepo<ImageRepositoryInterface, Image, ImageInterface>(store, kernel, ImageInterfaceExtensions.CopyFieldsFrom);

            //query service
            var imageQueryService = RepoUtil.SetupQueryService<ImageQueryServiceInterface, Image, ImageInterface>(store, kernel, ImageInterfaceExtensions.CopyFieldsFrom);

            //by browser
            RepoUtil.SetupQueryByBrowser<ImageQueryServiceInterface, Image, ImageInterface>(imageQueryService, store,
                                                                                              kernel,
                                                                                              ImageInterfaceExtensions.
                                                                                                  CopyFieldsFrom);
        }

        public static void SetUpWebsiteInfo(MoqMockingKernel kernel)
        {
            var websiteInfo = kernel.GetMock<WebsiteInfoServiceInterface>();



            websiteInfo.Setup(_ => _.GetBehaivourTags(It.IsAny<String>())).Returns("postaFlya");
            websiteInfo.Setup(_ => _.GetWebsiteName(It.IsAny<String>())).Returns("postaFlya");
            var tags = "event,social,comedy,theatre,books,pets,lost,found,services,music,fashion,food & drink,job,task,wanted,for sale,for free,sport,automotive,education,sale,garage,film,art & craft,photography,accommodation,technology,property,kids,politics";
            websiteInfo.Setup(_ => _.GetTags(It.IsAny<String>())).Returns(tags);
            //kernel.Rebind<WebsiteInfoServiceInterface>();
        }



        public static void SetUpBrowserRepositoryAndQueryService(MoqMockingKernel kernel, HashSet<BrowserInterface> store)
        {
            //repo
            var browserRepository = RepoUtil.SetupRepo<BrowserRepositoryInterface, Browser, BrowserInterface>(store, kernel, BrowserInterfaceExtensions.CopyFieldsFrom);

            //queryservice
            var browserQueryService = 
                RepoUtil.SetupQueryService<BrowserQueryServiceInterface, Browser, BrowserInterface>(store, kernel, BrowserInterfaceExtensions.CopyFieldsFrom);

            browserQueryService.Setup(m => m.FindByIdentityProvider(It.IsAny<IdentityProviderCredential>()))
                .Returns<IdentityProviderCredential>(prov =>
                    store.SingleOrDefault(bi =>
                        bi.ExternalCredentials != null &&
                        bi.ExternalCredentials.Any(ic => ic.Equals(prov))));

            browserQueryService.Setup(m => m.FindByHandle(It.IsAny<string>()))
                .Returns<string>(handle => store.SingleOrDefault(bi => bi.Handle == handle));

            AddMembersToStore(kernel, store);

        }

        public static void AddMembersToStore(StandardKernel kernel, ISet<BrowserInterface> mockStore)
        {
            mockStore.Add(kernel.Get<BrowserInterface>(ctx => ctx.Has("defaultbrowser")));
        }
    }
}


