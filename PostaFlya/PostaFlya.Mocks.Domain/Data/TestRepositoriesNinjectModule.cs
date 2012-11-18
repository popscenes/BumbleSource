using System.Collections.Generic;
using System.Linq;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.TaskJob;
using PostaFlya.Domain.TaskJob.Command;
using PostaFlya.Domain.TaskJob.Query;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Domain.Claims;
using Website.Domain.Comments;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Query;
using Website.Mocks.Domain.Data;

namespace PostaFlya.Mocks.Domain.Data
{

    public class TestRepositoriesNinjectModule : NinjectModule
    {
        public override void Load()
        {
            var kernel = Kernel as MoqMockingKernel;

            var boardFlierStore = RepoUtil.GetMockStore<BoardFlierInterface>();
            SetUpFlierRepositoryAndQueryService(kernel
                , RepoUtil.GetMockStore<FlierInterface>()
                , RepoUtil.GetMockStore<CommentInterface>()
                , RepoUtil.GetMockStore<ClaimInterface>()
                , boardFlierStore);
 
            SetUpTaskJobRepositoryAndQueryService(kernel
                , RepoUtil.GetMockStore<TaskJobFlierBehaviourInterface>()
                , RepoUtil.GetMockStore<TaskJobBidInterface>());

            SetUpBoardRepositoryAndQueryService(kernel
                , RepoUtil.GetMockStore<BoardInterface>());

            SetUpBoardFlierRepositoryAndQueryService(kernel
                , boardFlierStore);

        }

        public static  void SetUpBoardFlierRepositoryAndQueryService(MoqMockingKernel kernel, HashSet<BoardFlierInterface> store)
        {
            RepoUtil.SetupRepo<GenericRepositoryInterface, BoardFlier, BoardFlierInterface>(store, kernel, BoardFlierInterfaceExtensions.CopyFieldsFrom);

            /////////////query service
            RepoUtil.SetupQueryService<GenericQueryServiceInterface, BoardFlier, BoardFlierInterface>(store, kernel, BoardFlierInterfaceExtensions.CopyFieldsFrom);
            RepoUtil.FindAggregateEntities<GenericQueryServiceInterface, BoardFlier, BoardFlierInterface>(store, kernel,
                                                                                      BoardFlierInterfaceExtensions
                                                                                          .CopyFieldsFrom);
        }

        public static void SetUpBoardRepositoryAndQueryService(MoqMockingKernel kernel, HashSet<BoardInterface> store)
        {
            RepoUtil.SetupRepo<GenericRepositoryInterface, Board, BoardInterface>(store, kernel, BoardInterfaceExtensions.CopyFieldsFrom);

            /////////////query service
            RepoUtil.SetupQueryService<GenericQueryServiceInterface, Board, BoardInterface>(store, kernel, BoardInterfaceExtensions.CopyFieldsFrom);

        }

        public static void SetUpFlierRepositoryAndQueryService(MoqMockingKernel kernel
            , HashSet<FlierInterface> store
            , HashSet<CommentInterface> storeComment
            , HashSet<ClaimInterface> claimStore
            , HashSet<BoardFlierInterface> boardFlierStore )
        {
     
            ////////////repo
            var flierRepository = RepoUtil.SetupRepo<GenericRepositoryInterface, Flier, FlierInterface>(store, kernel, FlierInterfaceExtensions.CopyFieldsFrom);

            //comments
            //RepoUtil.SetupAddComment<FlierRepositoryInterface, FlierInterface, Flier>(repository, storeComment, kernel);

            //claimable
            //RepoUtil.SetupAddClaim<FlierRepositoryInterface, FlierInterface, Flier>(repository, claimStore, kernel);


            /////////////query service
            var flierQueryService = RepoUtil.SetupQueryService<QueryServiceForBrowserAggregateInterface, Flier, FlierInterface>(store, kernel, FlierInterfaceExtensions.CopyFieldsFrom);

            //by browser
            RepoUtil.SetupQueryByBrowser<QueryServiceForBrowserAggregateInterface, Flier, FlierInterface>(flierQueryService, store,
                                                                                              kernel,
                                                                                              FlierInterfaceExtensions.
                                                                                                  CopyFieldsFrom);

            //Comments
            RepoUtil.SetupRepo<GenericRepositoryInterface, Comment, CommentInterface>(storeComment, kernel, CommentInterfaceExtensions.CopyFieldsFrom);
            RepoUtil.SetupQueryService<QueryServiceForBrowserAggregateInterface, Comment, CommentInterface>(storeComment, kernel, CommentInterfaceExtensions.CopyFieldsFrom);

            RepoUtil.FindAggregateEntities<QueryServiceForBrowserAggregateInterface, Comment, CommentInterface>(storeComment, kernel,
                                                                                                  CommentInterfaceExtensions
                                                                                                      .CopyFieldsFrom);

            RepoUtil.SetupQueryByBrowser<QueryServiceForBrowserAggregateInterface, Comment, CommentInterface>(flierQueryService, storeComment,
                                                                                              kernel,
                                                                                              CommentInterfaceExtensions.
                                                                                                  CopyFieldsFrom);
            //RepoUtil.SetupQueryComments<FlierQueryServiceInterface, FlierInterface>(flierQueryService, storeComment, kernel);



            //claims
            RepoUtil.SetupRepo<GenericRepositoryInterface, Claim, ClaimInterface>(claimStore, kernel, ClaimInterfaceExtensions.CopyFieldsFrom);
            RepoUtil.SetupQueryService<QueryServiceForBrowserAggregateInterface, Claim, ClaimInterface>(claimStore, kernel, ClaimInterfaceExtensions.CopyFieldsFrom);

            RepoUtil.FindAggregateEntities<QueryServiceForBrowserAggregateInterface, Claim, ClaimInterface>(claimStore, kernel,
                                                                                      ClaimInterfaceExtensions
                                                                                          .CopyFieldsFrom);
            RepoUtil.SetupQueryByBrowser<QueryServiceForBrowserAggregateInterface, Claim, ClaimInterface>(flierQueryService, claimStore,
                                                                                  kernel,
                                                                                  ClaimInterfaceExtensions.
                                                                                      CopyFieldsFrom);

            //query by location
            var locationService = kernel.Get<LocationServiceInterface>();

            var flierSearchService = kernel.GetMock<FlierSearchServiceInterface>();
            flierSearchService.Setup(o => o.FindFliersByLocationTagsAndDistance(
                It.IsAny<Location>(), It.IsAny<Tags>(), It.IsAny<string>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<FlierSortOrder>(), It.IsAny<int>()))
                .Returns<Location, Tags, string, int, int, FlierSortOrder, int>(
                    (l, t, b, d, c, s, skip) =>
                    {
                        var boundingBox = d <= 0 ? locationService.GetDefaultBox(l) : 
                            locationService.GetBoundingBox(l, d);

                        var ret =  store
                            .Where(
                                f =>
                                locationService.IsWithinBoundingBox(boundingBox, f.Location) &&
                                f.Tags.Intersect(t).Any() &&
                                (string.IsNullOrWhiteSpace(b) 
                                || boardFlierStore.Any(
                                bf => bf.AggregateId == b && 
                                    bf.FlierId == f.Id && 
                                    bf.Status == BoardFlierStatus.Approved))
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


