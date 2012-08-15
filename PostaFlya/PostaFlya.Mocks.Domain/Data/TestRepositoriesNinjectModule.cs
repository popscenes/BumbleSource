using System;
using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Browser.Command;
using PostaFlya.Domain.Browser.Query;
using PostaFlya.Domain.Comments;
using PostaFlya.Domain.Comments.Command;
using PostaFlya.Domain.Comments.Query;
using PostaFlya.Domain.Content;
using PostaFlya.Domain.Content.Command;
using PostaFlya.Domain.Content.Query;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Likes;
using PostaFlya.Domain.Likes.Command;
using PostaFlya.Domain.Likes.Query;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.Service;
using PostaFlya.Domain.Tag;
using PostaFlya.Domain.TaskJob;
using PostaFlya.Domain.TaskJob.Command;
using PostaFlya.Domain.TaskJob.Query;
using WebSite.Infrastructure.Authentication;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Query;
using WebSite.Application.WebsiteInformation;

namespace PostaFlya.Mocks.Domain.Data
{

    public class TestRepositoriesNinjectModule : NinjectModule
    {
        public override void Load()
        {
            var kernel = Kernel as MoqMockingKernel;
            Assert.IsNotNull(kernel, "should be using mock kernel for tests");
            SetUpBrowserRepositoryAndQueryService(kernel, RepoUtil.GetMockStore<BrowserInterface>());
            SetUpFlierRepositoryAndQueryService(kernel
                , RepoUtil.GetMockStore<FlierInterface>()
                , RepoUtil.GetMockStore<CommentInterface>()
                , new List<LikeInterface>());
            SetUpImageRepositoryAndQueryService(kernel, RepoUtil.GetMockStore<ImageInterface>());   
            SetUpTaskJobRepositoryAndQueryService(kernel
                , RepoUtil.GetMockStore<TaskJobFlierBehaviourInterface>()
                , RepoUtil.GetMockStore<TaskJobBidInterface>());
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

        public static void SetUpFlierRepositoryAndQueryService(MoqMockingKernel kernel
            , HashSet<FlierInterface> store
            , HashSet<CommentInterface> storeComment
            , List<LikeInterface> likeStore)
        {

            

            ////////////repo
            var flierRepository = RepoUtil.SetupRepo<FlierRepositoryInterface, Flier, FlierInterface>(store, kernel, FlierInterfaceExtensions.CopyFieldsFrom);

            //comments
            RepoUtil.SetupAddComment<FlierRepositoryInterface, FlierInterface>(flierRepository, storeComment, kernel);

            //likeable
            RepoUtil.SetupAddLike<FlierRepositoryInterface, FlierInterface>(flierRepository, likeStore, kernel);


            /////////////query service
            var flierQueryService = RepoUtil.SetupQueryService<FlierQueryServiceInterface, Flier, FlierInterface>(store, kernel, FlierInterfaceExtensions.CopyFieldsFrom);


            //by browser
            RepoUtil.SetupQueryByBrowser<FlierQueryServiceInterface, Flier, FlierInterface>(flierQueryService, store,
                                                                                              kernel,
                                                                                              FlierInterfaceExtensions.
                                                                                                  CopyFieldsFrom);

            //Comments
            RepoUtil.SetupQueryComments<FlierQueryServiceInterface, FlierInterface>(flierQueryService, storeComment, kernel);



            //likeable
            RepoUtil.SetupQueryLike<FlierQueryServiceInterface, FlierInterface>(flierQueryService, likeStore, kernel);

            //query by location
            var locationService = kernel.Get<LocationServiceInterface>();

            flierQueryService.Setup(o => o.FindFliersByLocationTagsAndDistance(
                It.IsAny<Location>(), It.IsAny<Tags>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<FlierSortOrder>(), It.IsAny<int>()))
                .Returns<Location, Tags, int, int, FlierSortOrder, int>(
                    (l, t, d, c, s, skip) =>
                    {
                        var boundingBox = d <= 0 ? locationService.GetDefaultBox(l) : 
                            locationService.GetBoundingBox(l, d);

                        var ret =  store
                            .Where(
                                f =>
                                locationService.IsWithinBoundingBox(boundingBox, f.Location) &&
                                f.Tags.Intersect(t).Any()
                             )
                             .Select(f => f.Id);

                        if(skip > 0)
                            ret = ret.Skip(skip);

                        return c > 0 ? ret.Take(c).ToList() : ret.ToList();
                    }
                );


            //test data
            FlierTestData.AddSomeDataToMockFlierStore(flierRepository.Object, kernel);
            FlierTestData.AddSomeDataForHeatMapToMockFlierStore(flierRepository.Object, kernel);
         
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

        public static void SetUpTaskJobRepositoryAndQueryService(MoqMockingKernel kernel, 
            HashSet<TaskJobFlierBehaviourInterface> store
            ,HashSet<TaskJobBidInterface> bidstore)
        {
            var repository = RepoUtil.SetupRepo<TaskJobRepositoryInterface, TaskJobFlierBehaviour, TaskJobFlierBehaviourInterface>(store, kernel, TaskJobFlierBehavourInterfaceExtensions.CopyFieldsFrom);

            repository.Setup(o => o.BidOnTask(It.IsAny<TaskJobBidInterface>()))
                .Returns<TaskJobBidInterface>(flierBehaviour => bidstore.CopyAndStore<TaskJobBid, TaskJobBidInterface>(
                                                          flierBehaviour, TaskJobBidInterfaceExtensions.CopyFieldsFrom));
          

            repository.Setup(m => m.GetBidForUpdate(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((id, browserid) =>
                    bidstore.SingleOrDefault(b => b.TaskJobId == id && b.BrowserId == browserid));

            var taskJobQueryService =
                RepoUtil.SetupQueryService<TaskJobQueryServiceInterface, TaskJobFlierBehaviour, TaskJobFlierBehaviourInterface>(store, kernel, TaskJobFlierBehavourInterfaceExtensions.CopyFieldsFrom);

            taskJobQueryService.Setup(m => m.GetBids(It.IsAny<string>()))
                .Returns<string>(id => bidstore.Where(b => b.TaskJobId == id)
                                        .Select(bid => bid.CreateCopy<TaskJobBid, TaskJobBidInterface>(TaskJobBidInterfaceExtensions.CopyFieldsFrom))
                                        .AsQueryable());

        }
    }


}


