using System.Collections.Generic;
using System.Linq;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Analytic;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.TaskJob;
using Website.Domain.Browser.Query;
using Website.Domain.Payment;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Domain.Claims;
using Website.Domain.Comments;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Query;
using Website.Mocks.Domain.Data;
using Website.Test.Common;

namespace PostaFlya.Mocks.Domain.Data
{

    public class TestRepositoriesNinjectModule : NinjectModule
    {
        public override void Load()
        {
            var kernel = Kernel as MoqMockingKernel;

            var boardFlierStore = RepoCoreUtil.GetMockStore<BoardFlierInterface>();
            SetUpFlierRepositoryAndQueryService(kernel
                , RepoCoreUtil.GetMockStore<FlierInterface>()
                , RepoCoreUtil.GetMockStore<CommentInterface>()
                , RepoCoreUtil.GetMockStore<ClaimInterface>()
                , boardFlierStore
                , RepoCoreUtil.GetMockStore<PaymentTransactionInterface>()
                , RepoCoreUtil.GetMockStore<CreditTransactionInterface>());
 
            SetUpTaskJobRepositoryAndQueryService(kernel
                , RepoCoreUtil.GetMockStore<TaskJobFlierBehaviourInterface>()
                , RepoCoreUtil.GetMockStore<TaskJobBidInterface>());

            SetUpBoardRepositoryAndQueryService(kernel
                , RepoCoreUtil.GetMockStore<BoardInterface>());

            SetUpBoardFlierRepositoryAndQueryService(kernel
                , boardFlierStore);

            SetUpAnalyticRepositoryAndQueryService(kernel
                , RepoCoreUtil.GetMockStore<FlierAnalyticInterface>());

        }

        public static void SetUpAnalyticRepositoryAndQueryService(MoqMockingKernel kernel, HashSet<FlierAnalyticInterface> store)
        {
            RepoUtil.SetupRepo<GenericRepositoryInterface, FlierAnalytic, FlierAnalyticInterface>(store, kernel, FlierAnalyticInterfaceExtensions.CopyFieldsFrom);

            /////////////query service
            RepoUtil.SetupQueryService<GenericQueryServiceInterface, FlierAnalytic, FlierAnalyticInterface>(store, kernel, FlierAnalyticInterfaceExtensions.CopyFieldsFrom);
            RepoUtil.FindAggregateEntities<GenericQueryServiceInterface, FlierAnalytic, FlierAnalyticInterface>(store, kernel,
                                                                                      FlierAnalyticInterfaceExtensions
                                                                                          .CopyFieldsFrom);
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
            , HashSet<BoardFlierInterface> boardFlierStore
            , HashSet<PaymentTransactionInterface> paymentTransactionStore
            , HashSet<CreditTransactionInterface> creditTransactionStore)
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
            //payment transaction
            RepoUtil.SetupRepo<GenericRepositoryInterface, PaymentTransaction, PaymentTransactionInterface>(paymentTransactionStore, kernel, PaymentTransactionInterfaceExtensions.CopyFieldsFrom);

            RepoUtil.SetupQueryService<QueryServiceForBrowserAggregateInterface, PaymentTransaction, PaymentTransactionInterface>(paymentTransactionStore, kernel, 
                PaymentTransactionInterfaceExtensions.CopyFieldsFrom);
            RepoUtil.FindAggregateEntities<QueryServiceForBrowserAggregateInterface, PaymentTransaction, PaymentTransactionInterface>(paymentTransactionStore, kernel,
                                                                                                  PaymentTransactionInterfaceExtensions
                                                                                                      .CopyFieldsFrom);

            RepoUtil.SetupQueryByBrowser<QueryServiceForBrowserAggregateInterface, PaymentTransaction, PaymentTransactionInterface>(flierQueryService, paymentTransactionStore,
                                                                                              kernel,
                                                                                              PaymentTransactionInterfaceExtensions.
                                                                                                  CopyFieldsFrom);

            //payment transaction
            RepoUtil.SetupRepo<GenericRepositoryInterface, CreditTransaction, CreditTransactionInterface>(creditTransactionStore, kernel, CreditTransactionInterfaceExtensions.CopyFieldsFrom);

            RepoUtil.SetupQueryService<GenericQueryServiceInterface, CreditTransaction, CreditTransactionInterface>(creditTransactionStore, kernel,
                CreditTransactionInterfaceExtensions.CopyFieldsFrom);
            RepoUtil.FindAggregateEntities<QueryServiceForBrowserAggregateInterface, CreditTransaction, CreditTransactionInterface>(creditTransactionStore, kernel,
                                                                                                  CreditTransactionInterfaceExtensions
                                                                                                      .CopyFieldsFrom);


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
                                (t.Count == 0 || f.Tags.Intersect(t).Any()) &&
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
            var repository = RepoUtil.SetupRepo<GenericRepositoryInterface, TaskJobFlierBehaviour, TaskJobFlierBehaviourInterface>(store, kernel, TaskJobFlierBehavourInterfaceExtensions.CopyFieldsFrom);

            var taskJobQueryService =
                RepoUtil.SetupQueryService<QueryServiceForBrowserAggregateInterface, TaskJobFlierBehaviour, TaskJobFlierBehaviourInterface>(store, kernel, TaskJobFlierBehavourInterfaceExtensions.CopyFieldsFrom);

        }
    }


}


