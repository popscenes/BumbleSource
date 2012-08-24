using System;
using System.Collections.Generic;
using System.Linq;
using MbUnit.Framework;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.TaskJob;
using PostaFlya.Domain.TaskJob.Command;
using PostaFlya.Domain.TaskJob.Query;
using WebSite.Infrastructure.Authentication;
using WebSite.Infrastructure.Domain;
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
using Website.Mocks.Domain.Data;

namespace PostaFlya.Mocks.Domain.Data
{

    public class TestRepositoriesNinjectModule : NinjectModule
    {
        public override void Load()
        {
            var kernel = Kernel as MoqMockingKernel;

            SetUpFlierRepositoryAndQueryService(kernel
                , RepoUtil.GetMockStore<FlierInterface>()
                , RepoUtil.GetMockStore<CommentInterface>()
                , RepoUtil.GetMockStore<LikeInterface>());
 
            SetUpTaskJobRepositoryAndQueryService(kernel
                , RepoUtil.GetMockStore<TaskJobFlierBehaviourInterface>()
                , RepoUtil.GetMockStore<TaskJobBidInterface>());

        }

        public static void SetUpFlierRepositoryAndQueryService(MoqMockingKernel kernel
            , HashSet<FlierInterface> store
            , HashSet<CommentInterface> storeComment
            , HashSet<LikeInterface> likeStore)
        {

            

            ////////////repo
            var flierRepository = RepoUtil.SetupRepo<FlierRepositoryInterface, Flier, FlierInterface>(store, kernel, FlierInterfaceExtensions.CopyFieldsFrom);

            //comments
            //RepoUtil.SetupAddComment<FlierRepositoryInterface, FlierInterface, Flier>(flierRepository, storeComment, kernel);

            //likeable
            //RepoUtil.SetupAddLike<FlierRepositoryInterface, FlierInterface, Flier>(flierRepository, likeStore, kernel);


            /////////////query service
            var flierQueryService = RepoUtil.SetupQueryService<FlierQueryServiceInterface, Flier, FlierInterface>(store, kernel, FlierInterfaceExtensions.CopyFieldsFrom);


            //by browser
            RepoUtil.SetupQueryByBrowser<FlierQueryServiceInterface, Flier, FlierInterface>(flierQueryService, store,
                                                                                              kernel,
                                                                                              FlierInterfaceExtensions.
                                                                                                  CopyFieldsFrom);

            //Comments
            RepoUtil.SetupRepo<FlierRepositoryInterface, Comment, CommentInterface>(storeComment, kernel, CommentInterfaceExtensions.CopyFieldsFrom);
            RepoUtil.FindAggregateEntities<FlierQueryServiceInterface, Comment, CommentInterface>(storeComment, kernel,
                                                                                                  CommentInterfaceExtensions
                                                                                                      .CopyFieldsFrom);
            RepoUtil.SetupQueryByBrowser<FlierQueryServiceInterface, Comment, CommentInterface>(flierQueryService, storeComment,
                                                                                              kernel,
                                                                                              CommentInterfaceExtensions.
                                                                                                  CopyFieldsFrom);
            //RepoUtil.SetupQueryComments<FlierQueryServiceInterface, FlierInterface>(flierQueryService, storeComment, kernel);



            //likes
            RepoUtil.SetupRepo<FlierRepositoryInterface, Like, LikeInterface>(likeStore, kernel, LikeInterfaceExtensions.CopyFieldsFrom);
            RepoUtil.FindAggregateEntities<FlierQueryServiceInterface, Like, LikeInterface>(likeStore, kernel,
                                                                                      LikeInterfaceExtensions
                                                                                          .CopyFieldsFrom);
            RepoUtil.SetupQueryByBrowser<FlierQueryServiceInterface, Like, LikeInterface>(flierQueryService, likeStore,
                                                                                  kernel,
                                                                                  LikeInterfaceExtensions.
                                                                                      CopyFieldsFrom);

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


