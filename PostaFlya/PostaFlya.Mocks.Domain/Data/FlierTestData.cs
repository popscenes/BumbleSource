﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ninject;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Boards;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Venue;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Publish;
using Website.Infrastructure.Query;
//using Website.Infrastructure.Service;
using Website.Domain.Location;
using Website.Domain.Tag;
using Website.Mocks.Domain.Defaults;
using Website.Test.Common;

namespace PostaFlya.Mocks.Domain.Data
{
    public static class FlierTestData
    {
        public static Flier GetOne(IKernel kernel, Location loc = null)
        {
            if (loc == null)
                loc = kernel.Get<Location>(ib => ib.Get<bool>("default"));
            var ret = new Flier()
                       {
                           BrowserId = kernel.Get<string>(bm => bm.Has("defaultbrowserid")),
                           Title = "Test Title",
                           Description = "Test Description Yo",
                           Tags = kernel.Get<Tags>(bm => bm.Has("default")),
                           EffectiveDate = DateTime.Now.AddDays(20),
                           Image = Guid.NewGuid(),
                           FlierBehaviour = FlierBehaviour.TaskJob,
                           Status = FlierStatus.Active,
                           ImageList = new List<FlierImage>() { new FlierImage(Guid.NewGuid().ToString()), new FlierImage(Guid.NewGuid().ToString()), new FlierImage(Guid.NewGuid().ToString()) },
                           ExternalSource = "testsource",
                           ExternalId = "123",
                           //Venue = new VenueInformation(){PlaceName = "Venue", Address = loc},
                           TinyUrl = "http://tfly.in/" +  new Random().Next().ToString()
                           
                       };
            ret.FriendlyId = FlierQueryServiceUtil.FindFreeFriendlyIdForFlier(null, ret);
            return ret;
        }

//        public static FlierBehaviourInterface GetBehaviour(StandardKernel kernel, Flier ret)
//        {
//            var behaviourFactory = kernel.Get<BehaviourFactoryInterface>();
//            var queryService = kernel.Get<GenericQueryServiceInterface>();
//            var behaviour = behaviourFactory.GetDefaultBehaviourTypeForBehaviour(ret.FlierBehaviour);
//            var behave = queryService.FindById(behaviour, ret.Id) as FlierBehaviourInterface ??
//                         behaviourFactory.CreateBehaviourInstanceForFlier(ret);
//            behave.Flier = ret;
//            return behave;
//        }

        internal static void AddSomeDataForHeatMapToMockFlierStore(GenericRepositoryInterface flierRepository, IKernel kernel)
        {
            var board = BoardTestData.GetAndStoreOne(kernel, flierRepository,
                                                     loc: new Location(145.0138751, -37.8799136));
            //Jr's house
            for(var i = 0; i < 100; i++)
            {
                var flier = new Flier()
                {
                    BrowserId = Guid.NewGuid().ToString(),
                    Title = "Test Title",
                    Description = "Test Description Yo",
                    Tags = new Tags(new List<string>(){"HEATMAP"}),
                    EffectiveDate = DateTime.Now.AddDays(1),
                    Image = Guid.NewGuid(),
                    FlierBehaviour = FlierBehaviour.Default,
                    Status = FlierStatus.Active,
                    Boards = new List<BoardFlier>() { new BoardFlier(){BoardId = board.Id, DateAdded = DateTime.UtcNow, Status = BoardFlierStatus.Approved} }

                };

                flierRepository.Store(flier);
            }

            board = BoardTestData.GetAndStoreOne(kernel, flierRepository, boardName: "DisneyLand",
                                                     loc: new Location(-117.9189478, 33.8102936));
            //disneyland
            for (var i = 0; i < 50; i++)
            {
                var flier = new Flier()
                {
                    BrowserId = Guid.NewGuid().ToString(),
                    Title = "Test Title",
                    Description = "Test Description Yo",
                    Tags = new Tags(new List<string>() { "HEATMAP" }),
                    EffectiveDate = DateTime.Now.AddDays(1),
                    Image = Guid.NewGuid(),
                    FlierBehaviour = FlierBehaviour.Default,
                    Status = FlierStatus.Active,
                    Boards = new List<BoardFlier>() { new BoardFlier() { BoardId = board.Id, DateAdded = DateTime.UtcNow, Status = BoardFlierStatus.Approved } }

                    //Venue = new VenueInformation() { PlaceName = "Venue", Address = new Location(-117.9189478, 33.8102936) }

                };

                flierRepository.Store(flier);
            }

            board = BoardTestData.GetAndStoreOne(kernel, flierRepository, boardName: "Ricks",
                                                     loc: new Location(144.9770748, -37.7654897));

            //Ricks at casablanca
            for (var i = 0; i < 100; i++)
            {
                var flier = new Flier()
                {
                    BrowserId = Guid.NewGuid().ToString(),
                    Title = "Test Title",
                    Description = "Test Description Yo",
                    Tags = new Tags(new List<string>() { "HEATMAP" }),
                    EffectiveDate = DateTime.Now.AddDays(1),
                    Image = Guid.NewGuid(),
                    FlierBehaviour = FlierBehaviour.Default,
                    Status = FlierStatus.Active,
                    Boards = new List<BoardFlier>() { new BoardFlier() { BoardId = board.Id, DateAdded = DateTime.UtcNow, Status = BoardFlierStatus.Approved } }

                    //Venue = new VenueInformation() { PlaceName = "Venue", Address = new Location(144.9770748, -37.7654897) }

                };

                flierRepository.Store(flier);
            }
        }

        private static readonly Random Random = new Random();
        internal static Location GetRandomLocationInBoundingBox(BoundingBox box)
        {
            var latDif = Math.Abs(box.Max.Latitude - box.Min.Latitude);
            var longDif = Math.Abs(box.Max.Longitude - box.Min.Longitude);
            var ran = Random.Next(0, 1000);
            var latinc = latDif/1000;
            var longinc = longDif / 1000;
            var ret = new Location()
                       {
                           Longitude = box.Min.Longitude + (longinc * ran),
                           Latitude = box.Min.Latitude + (latinc * ran)
                       };

            Assert.IsTrue(IsWithinBoundingBox(box, ret));
            return ret;
        }

        public static bool IsWithinBoundingBox(BoundingBox boundingBox, Location location)
        {
            return location.Latitude >= boundingBox.Min.Latitude
               && location.Latitude <= boundingBox.Max.Latitude
               && location.Longitude >= boundingBox.Min.Longitude
               && location.Longitude <= boundingBox.Max.Longitude;
        }

        internal static Location GetRandomLocationOutsideBoundingBox(BoundingBox box)
        {
            Location ret;
            do
            {
                ret = new Location()
                          {
                              Longitude = (Random.Next(-1800000, 1800000)/10000.0),
                              Latitude = (Random.Next(-900000, 900000)/10000.0)
                          };
            } while (IsWithinBoundingBox(box, ret));

            Assert.IsFalse(IsWithinBoundingBox(box, ret));

            return ret;
        }

        internal static void AddSomeDataToMockFlierStore(GenericRepositoryInterface flierRepository, IKernel kernel)
        {
            var locationService = kernel.Get<LocationServiceInterface>();
            var defaultlocation = kernel.Get<Location>(ib => ib.Get<bool>("default"));
            var boundingBox = locationService.GetDefaultBox(defaultlocation);
            var latDif = Math.Abs(boundingBox.Max.Latitude - boundingBox.Min.Latitude);
            var longDif = Math.Abs(boundingBox.Max.Longitude - boundingBox.Min.Longitude);
            var random = new Random();

            Func<bool, Location> getRandLoc = (within) => within ?  GetRandomLocationInBoundingBox(boundingBox)
                                                                  : GetRandomLocationOutsideBoundingBox(boundingBox);

            var defaultTags = kernel.Get<Tags>(ib => ib.Get<bool>("default"));
            var otherTags = kernel.Get<Tags>(ib => ib.Get<bool>("someothertags"));
            Action<bool, Tags> getTags = (within, tags) =>
                                             {
                                                 //add some tags within the defaults
                                                 if(within)
                                                     tags.UnionWith(defaultTags.Take((random.Next()%defaultTags.Count) + 1));
                                                 //add some other non existent tags
                                                 tags.UnionWith(otherTags.Take((random.Next() % otherTags.Count + 1)));
                                             };


            var eventDates = new List<DateTimeOffset>() {new DateTime(2076, 8, 11), DateTime.UtcNow.AddDays(3)};
            //add inside the bounds with some matching tags
            var count = 0;
            var flier = new Flier() { EffectiveDate = DateTime.UtcNow, CreateDate = DateTime.UtcNow };
            getTags(true, flier.Tags);
            flier.BrowserId = GlobalDefaultsNinjectModule.DefaultBrowserId;
            flier.FriendlyId = "Bulletin" + count++;
            flier.EventDates = eventDates;
            var board = BoardTestData.GetOne(kernel, "Venue", BoardTypeEnum.VenueBoard, getRandLoc(true));
            BoardTestData.StoreOne(board, flierRepository, kernel);
            StoreOne(flier, flierRepository, (StandardKernel)kernel, board);

            flier = new Flier() {  EffectiveDate = DateTime.UtcNow, CreateDate = DateTime.UtcNow };
            getTags(true, flier.Tags);
            flier.BrowserId = GlobalDefaultsNinjectModule.DefaultBrowserId;
            flier.FriendlyId = "Bulletin" + count++;
            flier.EventDates = eventDates;
            board = BoardTestData.GetOne(kernel, "Venue", BoardTypeEnum.VenueBoard, getRandLoc(true));
            BoardTestData.StoreOne(board, flierRepository, kernel);
            StoreOne(flier, flierRepository, (StandardKernel)kernel, board);

            eventDates = new List<DateTimeOffset>() { new DateTime(2077, 12, 19), DateTime.UtcNow.AddDays(3) };
            flier = new Flier() { EffectiveDate = DateTime.UtcNow, CreateDate = DateTime.UtcNow };
            getTags(true, flier.Tags);
            flier.BrowserId = GlobalDefaultsNinjectModule.DefaultBrowserId;
            flier.FriendlyId = "Bulletin" + count++;                         
            flier.EventDates = eventDates;
            board = BoardTestData.GetOne(kernel, "Venue", BoardTypeEnum.VenueBoard, getRandLoc(true));
            BoardTestData.StoreOne(board, flierRepository, kernel);
            StoreOne(flier, flierRepository, (StandardKernel)kernel, board);

            //add inside the bounds without matching tags
            eventDates = new List<DateTimeOffset>() { new DateTime(2076, 8, 11), DateTime.UtcNow.AddDays(3) };
            flier = new Flier() { EffectiveDate = DateTime.UtcNow, CreateDate = DateTime.UtcNow };
            getTags(false, flier.Tags);
            flier.FriendlyId = "Bulletin" + count++;
            flier.EventDates = eventDates;
            board = BoardTestData.GetOne(kernel, "Venue", BoardTypeEnum.VenueBoard, getRandLoc(true));
            BoardTestData.StoreOne(board, flierRepository, kernel);
            StoreOne(flier, flierRepository, (StandardKernel) kernel, board);

            flier = new Flier() { EffectiveDate = DateTime.UtcNow, CreateDate = DateTime.UtcNow };
            getTags(false, flier.Tags);
            flier.FriendlyId = "Bulletin" + count++;
            flier.EventDates = eventDates;
            board = BoardTestData.GetOne(kernel, "Venue", BoardTypeEnum.VenueBoard, getRandLoc(true));
            BoardTestData.StoreOne(board, flierRepository, kernel);
            StoreOne(flier, flierRepository, (StandardKernel) kernel, board);

            flier = new Flier() { EffectiveDate = DateTime.UtcNow, CreateDate = DateTime.UtcNow };
            getTags(false, flier.Tags);
            flier.FriendlyId = "Bulletin" + count++;
            flier.EventDates = eventDates;
            board = BoardTestData.GetOne(kernel, "Venue", BoardTypeEnum.VenueBoard, getRandLoc(true));
            BoardTestData.StoreOne(board, flierRepository, kernel);
            StoreOne(flier, flierRepository, (StandardKernel)kernel, board);

            //add some outside the bounds with some matching tags
            flier = new Flier() { EffectiveDate = DateTime.UtcNow, CreateDate = DateTime.UtcNow };
            getTags(true, flier.Tags);
            flier.FriendlyId = "Bulletin" + count++;
            flier.EventDates = eventDates;
            board = BoardTestData.GetOne(kernel, "Venue", BoardTypeEnum.VenueBoard, getRandLoc(false));
            BoardTestData.StoreOne(board, flierRepository, kernel);
            StoreOne(flier, flierRepository, (StandardKernel)kernel, board);

            flier = new Flier() { EffectiveDate = DateTime.UtcNow, CreateDate = DateTime.UtcNow };
            getTags(true, flier.Tags);
            flier.FriendlyId = "Bulletin" + count++;
            flier.EventDates = eventDates;
            board = BoardTestData.GetOne(kernel, "Venue", BoardTypeEnum.VenueBoard, getRandLoc(false));
            BoardTestData.StoreOne(board, flierRepository, kernel);
            StoreOne(flier, flierRepository, (StandardKernel)kernel, board);

            flier = new Flier() { EffectiveDate = DateTime.UtcNow, CreateDate = DateTime.UtcNow };
            getTags(true, flier.Tags);
            flier.FriendlyId = "Bulletin" + count++;
            board = BoardTestData.GetOne(kernel, "Venue", BoardTypeEnum.VenueBoard, getRandLoc(false));
            BoardTestData.StoreOne(board, flierRepository, kernel);
            StoreOne(flier, flierRepository, (StandardKernel)kernel, board);

            //add some outside the bounds without matching tags
            flier = new Flier() { EffectiveDate = DateTime.UtcNow, CreateDate = DateTime.UtcNow };
            getTags(false, flier.Tags);
            flier.FriendlyId = "Bulletin" + count++;
            flier.EventDates = eventDates;
            board = BoardTestData.GetOne(kernel, "Venue", BoardTypeEnum.VenueBoard, getRandLoc(false));
            BoardTestData.StoreOne(board, flierRepository, kernel);
            StoreOne(flier, flierRepository, (StandardKernel)kernel, board);

            flier = new Flier() { EffectiveDate = DateTime.UtcNow, CreateDate = DateTime.UtcNow };
            getTags(false, flier.Tags);
            flier.FriendlyId = "Bulletin" + count++;
            flier.EventDates = eventDates;
            board = BoardTestData.GetOne(kernel, "Venue", BoardTypeEnum.VenueBoard, getRandLoc(false));
            BoardTestData.StoreOne(board, flierRepository, kernel);
            StoreOne(flier, flierRepository, (StandardKernel)kernel, board);

            flier = new Flier() { EffectiveDate = DateTime.UtcNow, CreateDate = DateTime.UtcNow };
            getTags(false, flier.Tags);
            flier.FriendlyId = "Bulletin" + count;
            flier.EventDates = eventDates;
            board = BoardTestData.GetOne(kernel, "Venue", BoardTypeEnum.VenueBoard, getRandLoc(false));
            BoardTestData.StoreOne(board, flierRepository, kernel);
            StoreOne(flier, flierRepository, (StandardKernel)kernel, board);
        }

        public static void AssertStoreRetrieve(FlierInterface storedFlier, FlierInterface retrievedFlier)
        {
            Assert.AreEqual(storedFlier.Id, retrievedFlier.Id);
            Assert.AreEqual(storedFlier.BrowserId, retrievedFlier.BrowserId);
            Assert.AreEqual(storedFlier.Title, retrievedFlier.Title);
            Assert.AreEqual(storedFlier.Description, retrievedFlier.Description);
            Assert.AreEqual(storedFlier.Image, retrievedFlier.Image);
            Assert.AreEqual(storedFlier.Status, retrievedFlier.Status);
            Assert.AreEqual(storedFlier.FlierBehaviour, retrievedFlier.FlierBehaviour);
            Assert.AreEqual(storedFlier.ExternalId, retrievedFlier.ExternalId);
            Assert.AreEqual(storedFlier.ExternalSource, retrievedFlier.ExternalSource);


            AssertUtil.AreEqual(storedFlier.CreateDate, storedFlier.CreateDate, TimeSpan.FromMilliseconds(1));
            AssertUtil.AreEqual(storedFlier.EffectiveDate, storedFlier.EffectiveDate, TimeSpan.FromMilliseconds(1));
            Assert.AreEqual(storedFlier.NumberOfComments, retrievedFlier.NumberOfComments);
            Assert.AreEqual(storedFlier.NumberOfClaims, retrievedFlier.NumberOfClaims);
            Assert.AreEqual(storedFlier.ImageList.Count, retrievedFlier.ImageList.Count);
            //Assert.HasSameElements
        }

        internal static FlierInterface AssertGetById(FlierInterface flier, GenericQueryServiceInterface queryService)
        {
            var retrievedFlier = queryService.FindById<Flier>(flier.Id);
            AssertStoreRetrieve(flier, retrievedFlier);

            return retrievedFlier;
        }


        internal static Flier StoreOnePublishEvent(Flier flier, GenericRepositoryInterface repository, IKernel kernel)
        {
            var uow = kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository });
            using (uow)
            {

                repository.Store(flier);
            }

            Assert.IsTrue(uow.Successful);


//            if (uow.Successful)
//            {
//                var domainEvent = kernel.Get<EventPublishServiceInterface>();
//                domainEvent.Publish(new FlierModifiedEvent() { Entity = (Flier)flier });
//            }


            return flier;
        }

        internal static Flier StoreOne(Flier flier, GenericRepositoryInterface repository, IKernel kernel, BoardInterface board = null)
        {
            if (board != null)
            {
                var exist = flier.Boards.SingleOrDefault(boardFlier => boardFlier.BoardId == board.Id);
                if (exist != null)
                    flier.Boards.Remove(exist);

                flier.Boards.Add(new BoardFlier() { BoardId = board.Id, BoardRank = 0, DateAdded = DateTime.UtcNow, Status = BoardFlierStatus.Approved });                             
            }
       
            if(!flier.Boards.Any())
                throw new Exception("need to have venue board");

            var uow = kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() {repository});
            using (uow)
            {

                repository.Store(flier);
            }

            Assert.IsTrue(uow.Successful);

//            if (uow.Successful)
//            {
//                var indexers = kernel.GetAll<HandleEventInterface<EntityModifiedEvent<Flier>>>();
//                foreach (var handleEvent in indexers)
//                {
//                    handleEvent.Handle(new FlierModifiedEvent() { Entity = (Flier)flier });
//                }
//            }

            
            return flier;
        }

        internal static void UpdateOne(FlierInterface flier, GenericRepositoryInterface repository, StandardKernel kernel)
        {
            Flier oldState = null;
            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = kernel.Get<UnitOfWorkFactoryInterface>().GetUnitOfWork(new List<RepositoryInterface>() {repository}))
            {
                repository.UpdateEntity<Flier>(flier.Id, e =>
                    {
                        oldState = e.CreateCopy<Flier, Flier>(FlierInterfaceExtensions.CopyFieldsFrom);
                        e.CopyFieldsFrom(flier);
                    });
            }

//            if (unitOfWork.Successful)
//            {
//                var indexers = kernel.GetAll<HandleEventInterface<EntityModifiedEvent<Flier>>>();
//                foreach (var handleEvent in indexers)
//                {
//                    handleEvent.Handle(new FlierModifiedEvent() { Entity = (Flier)flier });
//                }
//            }
        }
    }
}
