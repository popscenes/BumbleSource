﻿using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Ninject;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Boards.Query;
using PostaFlya.Domain.Browser;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Analytic;
using PostaFlya.Domain.Flier.Query;
using Website.Domain.Browser.Query;
using Website.Domain.Payment;
using Website.Infrastructure.Authentication;
using Website.Infrastructure.Command;
using Website.Domain.Claims;
using Website.Domain.Comments;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Infrastructure.Query;
using Website.Mocks.Domain.Data;
using Website.Mocks.Domain.Defaults;
using Website.Test.Common;
using BrowserIdentityProviderCredential = Website.Domain.Browser.BrowserIdentityProviderCredential;
using BrowserIdentityProviderCredentialInterface = Website.Domain.Browser.BrowserIdentityProviderCredentialInterface;
using BrowserIdentityProviderCredentialInterfaceExtensions = Website.Domain.Browser.BrowserIdentityProviderCredentialInterfaceExtensions;
using Role = Website.Domain.Browser.Role;
using Roles = Website.Domain.Browser.Roles;


namespace PostaFlya.Mocks.Domain.Data
{

    public class TestRepositoriesNinjectModule : NinjectModule
    {
        public override void Load()
        {
            var kernel = Kernel as MoqMockingKernel;

            SetUpFlierRepositoryAndQueryService(kernel
                , RepoCoreUtil.GetMockStore<FlierInterface>()
                , RepoCoreUtil.GetMockStore<CommentInterface>()
                , RepoCoreUtil.GetMockStore<ClaimInterface>()
                , RepoCoreUtil.GetMockStore<PaymentTransactionInterface>()
                , RepoCoreUtil.GetMockStore<CreditTransactionInterface>());
 

            SetUpBoardRepositoryAndQueryService(kernel
                , RepoCoreUtil.GetMockStore<BoardInterface>());


            SetUpAnalyticRepositoryAndQueryService(kernel
                , RepoCoreUtil.GetMockStore<FlierAnalyticInterface>());

            SetUpBrowserRepositoryAndQueryService(kernel
                , RepoCoreUtil.GetMockStore<BrowserInterface>()
                , RepoCoreUtil.GetMockStore<BrowserIdentityProviderCredentialInterface>());

        }

        public static void SetUpAnalyticRepositoryAndQueryService(MoqMockingKernel kernel, HashSet<FlierAnalyticInterface> store)
        {
            RepoCoreUtil.SetupAggregateRepo<GenericRepositoryInterface, FlierAnalytic, FlierAnalyticInterface, FlierAnalyticInterface>(store, kernel, FlierAnalyticInterfaceExtensions.CopyFieldsFrom);

            /////////////query service
            RepoUtil.FindAggregateEntities<GenericQueryServiceInterface, FlierAnalytic, FlierAnalyticInterface>(store, kernel,
                                                                                      FlierAnalyticInterfaceExtensions
                                                                                          .CopyFieldsFrom);
        }


        public static void SetUpBoardRepositoryAndQueryService(MoqMockingKernel kernel, HashSet<BoardInterface> store)
        {
            RepoUtil.SetupRepo<GenericRepositoryInterface, Board, BoardInterface>(store, kernel, BoardInterfaceExtensions.CopyFieldsFrom);

            /////////////query service
            RepoUtil.SetupQueryService<GenericQueryServiceInterface, Board, BoardInterface, BoardInterface>(store, kernel, BoardInterfaceExtensions.CopyFieldsFrom);


            //query by location
            var locationService = kernel.Get<LocationServiceInterface>();

            var findBoardsNearQueryHandler = kernel.GetMock<QueryHandlerInterface<FindBoardsNearQuery, List<string>>>();
            findBoardsNearQueryHandler
                .Setup(handler => handler.Query(It.IsAny<FindBoardsNearQuery>()))
                .Returns<FindBoardsNearQuery>(query =>
                    {
                        var boundingBox = locationService.GetBoundingBox(query.Location, query.WithinMetres / 1000.0);
                        var ret = store
                        .Where(
                            f => f.InformationSources.First().Address != null && f.InformationSources.First().Address.IsValid && f.BoardTypeEnum != BoardTypeEnum.InterestBoard &&
                            locationService.IsWithinBoundingBox(boundingBox, f.InformationSources.First().Address)
                         )
                         .Select(f => f.Id);

                        return ret.ToList();

                    });

        }

        public static void SetUpFlierRepositoryAndQueryService(MoqMockingKernel kernel
            , HashSet<FlierInterface> store
            , HashSet<CommentInterface> storeComment
            , HashSet<ClaimInterface> claimStore
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
            var flierQueryService = RepoUtil.SetupQueryService<GenericQueryServiceInterface, Flier, FlierInterface, FlierInterface>(store, kernel, FlierInterfaceExtensions.CopyFieldsFrom);

            //by browser
//            RepoUtil.SetupQueryByBrowser<GenericQueryServiceInterface, Flier, FlierInterface>(flierQueryService, store,
//                                                                                              kernel,
//                                                                                              FlierInterfaceExtensions.
//                                                                                                  CopyFieldsFrom);

            //Comments
            RepoCoreUtil.SetupAggregateRepo<GenericRepositoryInterface, Comment, CommentInterface, CommentInterface>(storeComment, kernel, CommentInterfaceExtensions.CopyFieldsFrom);

            RepoUtil.FindAggregateEntities<GenericQueryServiceInterface, Comment, CommentInterface>(storeComment, kernel,
                                                                                                  CommentInterfaceExtensions
                                                                                                      .CopyFieldsFrom);

//            RepoUtil.SetupQueryByBrowser<GenericQueryServiceInterface, Comment, CommentInterface>(flierQueryService, storeComment,
//                                                                                              kernel,
//                                                                                              CommentInterfaceExtensions.
//                                                                                                  CopyFieldsFrom);
            //RepoUtil.SetupQueryComments<FlierQueryServiceInterface, FlierInterface>(flierQueryService, storeComment, kernel);



            //claims
            RepoCoreUtil.SetupAggregateRepo<GenericRepositoryInterface, Claim, ClaimInterface, ClaimInterface>(claimStore, kernel, ClaimInterfaceExtensions.CopyFieldsFrom);

            RepoUtil.FindAggregateEntities<GenericQueryServiceInterface, Claim, ClaimInterface>(claimStore, kernel,
                                                                                      ClaimInterfaceExtensions
                                                                                          .CopyFieldsFrom);
//            RepoUtil.SetupQueryByBrowser<GenericQueryServiceInterface, Claim, ClaimInterface>(flierQueryService, claimStore,
//                                                                                  kernel,
//                                                                                  ClaimInterfaceExtensions.
//                                                                                      CopyFieldsFrom);
            //payment transaction
            RepoCoreUtil.SetupAggregateRepo<GenericRepositoryInterface, PaymentTransaction, PaymentTransactionInterface, PaymentTransactionInterface>(paymentTransactionStore, kernel, PaymentTransactionInterfaceExtensions.CopyFieldsFrom);

            RepoCoreUtil.SetupAggregateQuery<GenericQueryServiceInterface, PaymentTransaction, PaymentTransactionInterface>(paymentTransactionStore, kernel, 
                PaymentTransactionInterfaceExtensions.CopyFieldsFrom);
            RepoUtil.FindAggregateEntities<GenericQueryServiceInterface, PaymentTransaction, PaymentTransactionInterface>(paymentTransactionStore, kernel,
                                                                                                  PaymentTransactionInterfaceExtensions
                                                                                                      .CopyFieldsFrom);

//            RepoUtil.SetupQueryByBrowser<GenericQueryServiceInterface, PaymentTransaction, PaymentTransactionInterface>(flierQueryService, paymentTransactionStore,
//                                                                                              kernel,
//                                                                                              PaymentTransactionInterfaceExtensions.
//                                                                                                  CopyFieldsFrom);

            //payment transaction
            RepoCoreUtil.SetupAggregateRepo<GenericRepositoryInterface, CreditTransaction, CreditTransactionInterface, CreditTransactionInterface>(creditTransactionStore, kernel, CreditTransactionInterfaceExtensions.CopyFieldsFrom);

            RepoUtil.FindAggregateEntities<GenericQueryServiceInterface, CreditTransaction, CreditTransactionInterface>(creditTransactionStore, kernel,
                                                                                                  CreditTransactionInterfaceExtensions
                                                                                                      .CopyFieldsFrom);


            //query by location
            var locationService = kernel.Get<LocationServiceInterface>();

            var flierSearchService = kernel.GetMock<FlierSearchServiceInterface>();
            
            flierSearchService.Setup(o => o.FindFliersByLocationAndDistance(
                It.IsAny<Location>(),
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<FlierInterface>(), It.IsAny<Tags>(),  It.IsAny<DateTime?>(), It.IsAny<FlierSortOrder>()))
                .Returns<Location, int, int, FlierInterface, Tags, DateTime?, FlierSortOrder>(
                    (l, d, c, skip, t, dt, s) =>
                    {
                        var boundingBox = d <= 0 ? locationService.GetDefaultBox(l) : 
                            locationService.GetBoundingBox(l, d);

                        var ret = store
                            .Where(
                                f =>
                                    {
                                        var queryService = kernel.Get<GenericQueryServiceInterface>();
                                        var venuBoard = queryService.FindByIds<Board>(f.Boards.Select(b => b.BoardId))
                                            .First(board => board.Venue() != null);
                                        return
                                            locationService.IsWithinBoundingBox(boundingBox, venuBoard.Venue().Address) &&
                                            (t.Count == 0 || f.Tags.Union(t).Any()) &&
                                            (dt == null || f.EventDates.Any(ed => ed == dt.Value));
                                    }

                             )
                             .Select(f => f.Id);

                        if (skip != null)
                        {
                            var skippast = 0;
                            ret = ret.SkipWhile(s1 => s1 != skip.Id || skippast++ == 0);
                                                        
                        }

                        return c > 0 ? ret.Take(c).ToList() : ret.ToList();
                    }
                );

            flierSearchService.Setup(o => o.FindFliersByBoard(It.IsAny<string>(),
                                                              It.IsAny<int>(), It.IsAny<FlierInterface>(), It.IsAny<DateTime?>(),
                                                              It.IsAny<Tags>(), It.IsAny<FlierSortOrder>()
                                                              , It.IsAny<Location>(), It.IsAny<int>()))
                              .Returns<string, int, FlierInterface, DateTime?, Tags, FlierSortOrder, Location, int>(
                                  (b, c, skip, dt, t, s, l, d) =>
                                      {
                                          var boundingBox = (l == null || !l.IsValid)
                                                                ? null
                                                                : (d <= 0
                                                                       ? locationService.GetDefaultBox(l)
                                                                       : locationService.GetBoundingBox(l, d));

                                          var ret = store
                                              .Where(
                                                  f =>
                                                      {
                                                          var queryService = kernel.Get<GenericQueryServiceInterface>();
                                                          var venuBoard = queryService.FindByIds<Board>(f.Boards.Select(brd => brd.BoardId))
                                                                    .First(board => board.Venue() != null);
                                                          return (boundingBox == null ||
                                                                  locationService.IsWithinBoundingBox(boundingBox,
                                                                                                      venuBoard.Venue().Address)) &&
                                                                 (t.Count == 0 || f.Tags.Intersect(t).Any()) &&
                                                                 (dt == null || f.EventDates.Any(ed => ed == dt.Value)) &&
                                                                 (f.Boards.Any(
                                                                     bf => bf.BoardId == b &&
                                                                           bf.Status == BoardFlierStatus.Approved));
                                                      }

                                                            
                                              )
                                              .Select(f => f.Id);

                                          if (skip != null)
                                          {
                                              var skippast = 0;
                                              ret = ret.SkipWhile(s1 => s1 != skip.Id || skippast++ == 0);

                                          }

                                          return c > 0 ? ret.Take(c).ToList() : ret.ToList();
                                      }
                );

            //test data
            FlierTestData.AddSomeDataToMockFlierStore(flierRepository.Object, kernel);
            FlierTestData.AddSomeDataForHeatMapToMockFlierStore(flierRepository.Object, kernel);
         
        }



//        public static void SetUpTaskJobRepositoryAndQueryService(MoqMockingKernel kernel, 
//            HashSet<TaskJobFlierBehaviourInterface> store
//            ,HashSet<TaskJobBidInterface> bidstore)
//        {
//            var repository = RepoUtil.SetupRepo<GenericRepositoryInterface, TaskJobFlierBehaviour, TaskJobFlierBehaviourInterface>(store, kernel, TaskJobFlierBehavourInterfaceExtensions.CopyFieldsFrom);
//
//            var taskJobQueryService =
//                RepoUtil.SetupQueryService<GenericQueryServiceInterface, TaskJobFlierBehaviour, TaskJobFlierBehaviourInterface>(store, kernel, TaskJobFlierBehavourInterfaceExtensions.CopyFieldsFrom);
//
//        }

        public static void SetUpBrowserRepositoryAndQueryService(MoqMockingKernel kernel
          , HashSet<BrowserInterface> store
          , HashSet<BrowserIdentityProviderCredentialInterface> credStore)
        {

            //repo
//            var browserCredRepository = RepoUtil.SetupRepo<GenericRepositoryInterface, BrowserIdentityProviderCredential, BrowserIdentityProviderCredentialInterface>(credStore, kernel, BrowserIdentityProviderCredentialInterfaceExtensions.CopyFieldsFrom);

            //queryservice
//            var browserCredQueryService =
//                RepoUtil.SetupQueryService<GenericQueryServiceInterface, BrowserIdentityProviderCredential, BrowserIdentityProviderCredentialInterface, BrowserIdentityProviderCredentialInterface>(credStore, kernel, BrowserIdentityProviderCredentialInterfaceExtensions.CopyFieldsFrom);

            //repo
            var browserRepository = RepoUtil.SetupRepo<GenericRepositoryInterface, Browser, BrowserInterface>(store, kernel, BrowserInterfaceExtensions.CopyFieldsFrom);

            //queryservice
            var browserQueryService =
                RepoUtil.SetupQueryService<GenericQueryServiceInterface, Browser, BrowserInterface, BrowserInterface>(store, kernel, BrowserInterfaceExtensions.CopyFieldsFrom);
            RepoUtil.SetupQueryService<GenericQueryServiceInterface, Website.Domain.Browser.Browser, Website.Domain.Browser.BrowserInterface, BrowserInterface>(store, kernel, Website.Domain.Browser.BrowserInterfaceExtensions.CopyFieldsFrom);

            //            browserQueryService.Setup(m => m.FindBrowserByIdentityProvider(It.IsAny<IdentityProviderCredential>()))
            //                .Returns<IdentityProviderCredential>(prov =>
            //                    store.SingleOrDefault(bi =>
            //                        bi.ExternalCredentials != null &&
            //                        bi.ExternalCredentials.Any(ic => ic.Equals(prov))));

            //            browserQueryService.Setup(m => m.FindByHandle(It.IsAny<string>()))
            //                .Returns<string>(handle => store.SingleOrDefault(bi => bi.Handle == handle));

            AddMembersToStore(kernel, store);

        }

        public static void AddMembersToStore(IKernel kernel, ISet<BrowserInterface> mockStore)
        {
            AddBrowsers(kernel);
            mockStore.Add(kernel.Get<BrowserInterface>(ctx => ctx.Has("postadefaultbrowser")));
        }

        public static void AddBrowsers(IKernel kernel)
        {
            var token = new AccessToken()
            {
                Expires = DateTime.Now,
                Permissions = "post",
                Token = "123abc"
            };

            kernel.Bind<BrowserInterface>().ToMethod(ctx =>
                new PostaFlya.Domain.Browser.Browser()
                {
                    Id = GlobalDefaultsNinjectModule.DefaultBrowserId,
                    FriendlyId = "anthonyborg",
                    EmailAddress = "teddymccuddles@gmail.com",
                    Roles = new Roles { Role.Participant.ToString() },
                    ExternalCredentials = new HashSet<BrowserIdentityProviderCredential>()
                                                            {
                                                                new BrowserIdentityProviderCredential()
                                                                    {
                                                                        BrowserId = GlobalDefaultsNinjectModule.DefaultBrowserId,
                                                                        IdentityProvider = IdentityProviders.GOOGLE,
                                                                        UserIdentifier = "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1ho"
                                                                    }
                                                            }

                })
                .WithMetadata("postastsbrowser", true);

            kernel.Bind<BrowserInterface>().ToMethod(ctx =>
                new PostaFlya.Domain.Browser.Browser()
                {
                    Id = GlobalDefaultsNinjectModule.DefaultBrowserId,
                    FriendlyId = "rickyaudsley",
                    EmailAddress = "rickyaudlsey@gmail.com",
                    FirstName = "Ricky",
                    Surname = "Audsley",
                    PhoneNumber = "0411111111",
                    Roles = new Roles { Role.Participant.ToString() },
                    ExternalCredentials = new HashSet<BrowserIdentityProviderCredential>()
                                                            {
                                                                new BrowserIdentityProviderCredential()
                                                                    {
                                                                        BrowserId = GlobalDefaultsNinjectModule.DefaultBrowserId,
                                                                        IdentityProvider = IdentityProviders.GOOGLE, 
                                                                        UserIdentifier = "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1hoBlah",
                                                                        AccessToken =  token
                                                                    },

                                                                    new BrowserIdentityProviderCredential()
                                                                    {
                                                                        BrowserId = GlobalDefaultsNinjectModule.DefaultBrowserId,
                                                                        IdentityProvider = IdentityProviders.FACEBOOK, 
                                                                        UserIdentifier = "AItOawnldHWXFZoFpHDwBAMy34d1aO7qHSPz1hoBlah",
                                                                        AccessToken =  token
                                                                    }
                                                            }

                })
                .WithMetadata("postadefaultbrowser", true);
        }

    }


}


