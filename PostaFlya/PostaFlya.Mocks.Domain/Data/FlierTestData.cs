using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Ninject;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Flier.Command;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Service;
using Website.Infrastructure.Command;
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
            return new Flier(loc)
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
                           ExternalId = "123"
                           
                       };
        }

        public static FlierBehaviourInterface GetBehaviour(StandardKernel kernel, Flier ret)
        {
            var behaviourFactory = kernel.Get<BehaviourFactoryInterface>();
            var queryService = kernel.Get<GenericQueryServiceInterface>();
            var behaviour = behaviourFactory.GetDefaultBehaviourTypeForBehaviour(ret.FlierBehaviour);
            var behave = queryService.FindById(behaviour, ret.Id) as FlierBehaviourInterface ??
                         behaviourFactory.CreateBehaviourInstanceForFlier(ret);
            behave.Flier = ret;
            return behave;
        }

        internal static void AddSomeDataForHeatMapToMockFlierStore(FlierRepositoryInterface flierRepository, IKernel kernel)
        {
            //Jr's house
            for(var i = 0; i < 100; i++)
            {
                var flier = new Flier(new Location(145.0138751 ,-37.8799136))
                {
                    BrowserId = Guid.NewGuid().ToString(),
                    Title = "Test Title",
                    Description = "Test Description Yo",
                    Tags = new Tags(new List<string>(){"HEATMAP"}),
                    EffectiveDate = DateTime.Now.AddDays(1),
                    Image = Guid.NewGuid(),
                    FlierBehaviour = FlierBehaviour.Default,
                    Status = FlierStatus.Active
                };

                flierRepository.Store(flier);
            }

            //disneyland
            for (var i = 0; i < 50; i++)
            {
                var flier = new Flier(new Location(-117.9189478,33.8102936))
                {
                    BrowserId = Guid.NewGuid().ToString(),
                    Title = "Test Title",
                    Description = "Test Description Yo",
                    Tags = new Tags(new List<string>() { "HEATMAP" }),
                    EffectiveDate = DateTime.Now.AddDays(1),
                    Image = Guid.NewGuid(),
                    FlierBehaviour = FlierBehaviour.Default,
                    Status = FlierStatus.Active
                };

                flierRepository.Store(flier);
            }

            //Ricks at casablanca
            for (var i = 0; i < 100; i++)
            {
                var flier = new Flier(new Location(144.9770748, -37.7654897))
                {
                    BrowserId = Guid.NewGuid().ToString(),
                    Title = "Test Title",
                    Description = "Test Description Yo",
                    Tags = new Tags(new List<string>() { "HEATMAP" }),
                    EffectiveDate = DateTime.Now.AddDays(1),
                    Image = Guid.NewGuid(),
                    FlierBehaviour = FlierBehaviour.Default,
                    Status = FlierStatus.Active
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

        internal static void AddSomeDataToMockFlierStore(FlierRepositoryInterface flierRepository, IKernel kernel)
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
                

            //add inside the bounds with some matching tags
            var flier = new Flier(getRandLoc(true)) {EffectiveDate = DateTime.Now};
            getTags(true, flier.Tags);
            flier.BrowserId = GlobalDefaultsNinjectModule.DefaultBrowserId;
            flierRepository.Store(flier);

            flier = new Flier(getRandLoc(true)) {EffectiveDate = DateTime.Now.AddDays(-1)};
            getTags(true, flier.Tags);
            flier.BrowserId = GlobalDefaultsNinjectModule.DefaultBrowserId;
            flierRepository.Store(flier);

            flier = new Flier(getRandLoc(true)) {EffectiveDate = DateTime.Now.AddDays(3)};
            getTags(true, flier.Tags);
            flier.BrowserId = GlobalDefaultsNinjectModule.DefaultBrowserId;
            flierRepository.Store(flier);

            //add inside the bounds without matching tags
            flier = new Flier(getRandLoc(true)) {EffectiveDate = DateTime.Now.AddDays(0)};
            getTags(false, flier.Tags);
            flierRepository.Store(flier);
            flier = new Flier(getRandLoc(true)) {EffectiveDate = DateTime.Now.AddDays(-1)};
            getTags(false, flier.Tags);
            flierRepository.Store(flier);
            flier = new Flier(getRandLoc(true)) {EffectiveDate = DateTime.Now.AddDays(3)};
            getTags(false, flier.Tags);
            flierRepository.Store(flier);

            //add some outside the bounds with some matching tags
            flier = new Flier(getRandLoc(false)) {EffectiveDate = DateTime.Now.AddDays(0)};
            getTags(true, flier.Tags);
            flierRepository.Store(flier);
            flier = new Flier(getRandLoc(false)) {EffectiveDate = DateTime.Now.AddDays(1)};
            getTags(true, flier.Tags);
            flierRepository.Store(flier);
            flier = new Flier(getRandLoc(false)) {EffectiveDate = DateTime.Now.AddDays(-3)};
            getTags(true, flier.Tags);
            flierRepository.Store(flier);

            //add some outside the bounds without matching tags
            flier = new Flier(getRandLoc(false)) {EffectiveDate = DateTime.Now.AddDays(0)};
            getTags(false, flier.Tags);
            flierRepository.Store(flier);
            flier = new Flier(getRandLoc(false)) {EffectiveDate = DateTime.Now.AddDays(1)};
            getTags(false, flier.Tags);
            flierRepository.Store(flier);
            flier = new Flier(getRandLoc(false)) {EffectiveDate = DateTime.Now.AddDays(-3)};
            getTags(false, flier.Tags);
            flierRepository.Store(flier);
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

        internal static FlierInterface AssertGetById(FlierInterface flier, FlierQueryServiceInterface queryService)
        {
            var retrievedFlier = queryService.FindById<Flier>(flier.Id);
            AssertStoreRetrieve(flier, retrievedFlier);

            return retrievedFlier;
        }


        internal static Flier StoreOne(Flier flier, GenericRepositoryInterface repository, StandardKernel kernel)
        {
            var uow = kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() {repository});
            using (uow)
            {

                repository.Store(flier);
            }

            Assert.IsTrue(uow.Successful);
            return flier;
        }

        internal static void UpdateOne(FlierInterface flier, FlierRepositoryInterface repository, StandardKernel kernel)
        {
            using (kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository }))
            {
                repository.UpdateEntity<Flier>(flier.Id, e => e.CopyFieldsFrom(flier));
            }
        }
    }
}
