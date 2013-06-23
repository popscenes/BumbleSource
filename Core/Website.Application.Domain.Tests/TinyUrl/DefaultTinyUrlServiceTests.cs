using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Domain.Browser;
using Website.Application.Domain.TinyUrl;
using Website.Application.Queue;
using Website.Application.Tests.Mocks;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;
using Website.Test.Common;

namespace Website.Application.Domain.Tests.TinyUrl
{
    [TestFixture]
    public class DefaultTinyUrlServiceTests
    {
        static MoqMockingKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        readonly HashSet<TinyUrlRecordInterface> _store = RepoCoreUtil.GetMockStore<TinyUrlRecordInterface>();
        readonly HashSet<EntityWithTinyUrlInterface> _entityStore = RepoCoreUtil.GetMockStore<EntityWithTinyUrlInterface>();

        
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {           
            RepoCoreUtil.SetupAggregateRepo<GenericRepositoryInterface, TinyUrlRecord, TinyUrlRecordInterface, TinyUrlRecordInterface>(_store, Kernel, TinyUrlRecordInterfaceExtensions.CopyFieldsFrom);
            RepoCoreUtil.SetupAggregateQuery<GenericQueryServiceInterface, TinyUrlRecord, TinyUrlRecordInterface>(_store, Kernel, TinyUrlRecordInterfaceExtensions.CopyFieldsFrom);

            RepoCoreUtil.SetupRepo<GenericRepositoryInterface, EntityKeyWithTinyUrl, EntityWithTinyUrlInterface, EntityWithTinyUrlInterface>(_entityStore, Kernel, EntityWithTinyUrlInterfaceExtensions.CopyFieldsFrom);
            RepoCoreUtil.SetupQueryService<GenericQueryServiceInterface, EntityKeyWithTinyUrl, EntityWithTinyUrlInterface, EntityWithTinyUrlInterface>(_entityStore, Kernel, EntityWithTinyUrlInterfaceExtensions.CopyFieldsFrom);

            ReInit();
        }
 
        private  void ReInit()
        {
            _store.Clear();
            _entityStore.Clear();
            
            var repo = Kernel.Get<GenericRepositoryInterface>();
            repo.Store(new TinyUrlRecord()
            {
                AggregateId = TinyUrlRecord.UnassignedToAggregateId,
                AggregateTypeTag = "",
                FriendlyId = "",
                Id = TinyUrlRecord.GenerateIdFromUrl("http://tin.y/1"),
                TinyUrl = "http://tin.y/1"
            });

            repo.Store(new TinyUrlRecord()
            {
                AggregateId = TinyUrlRecord.UnassignedToAggregateId,
                AggregateTypeTag = "",
                FriendlyId = "",
                Id = TinyUrlRecord.GenerateIdFromUrl("http://tin.y/2"),
                TinyUrl = "http://tin.y/2"
            });

            repo.Store(new TinyUrlRecord()
            {
                AggregateId = TinyUrlRecord.UnassignedToAggregateId,
                AggregateTypeTag = "",
                FriendlyId = "",
                Id = TinyUrlRecord.GenerateIdFromUrl("http://tin.y/3"),
                TinyUrl = "http://tin.y/3"
            });

        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<QueueInterface>();
        }


        [Test]
        public void DefaultTinyUrlServiceUrlForReturnsANewUrlForANewEntityTest()
        {
            ReInit();
            var urlService = Kernel.Get<DefaultTinyUrlService>();
            var entity = new EntityKeyWithTinyUrl() {Id = "TestId"};
            entity.TinyUrl = urlService.UrlFor(entity);
            Assert.IsNotNullOrEmpty(entity.TinyUrl);
            StoreEntity(entity);

            var ret = urlService.EntityInfoFor(entity.TinyUrl);
            Assert.That(ret, Is.Not.Null);
            Assert.That(ret.Id, Is.EqualTo(entity.Id));


            var entityTwo = new EntityKeyWithTinyUrl() { Id = "TestIdTwo" };
            entityTwo.TinyUrl = urlService.UrlFor(entityTwo);
            Assert.IsNotNullOrEmpty(entityTwo.TinyUrl);
            StoreEntity(entityTwo);
            
            ret = urlService.EntityInfoFor(entityTwo.TinyUrl);
            Assert.That(ret, Is.Not.Null);
            Assert.That(ret.Id, Is.EqualTo(entityTwo.Id));



            Assert.AreNotEqual(entity.TinyUrl, entityTwo.TinyUrl);
        }

        [Test]
        public void DefaultTinyUrlServiceUrlForReturnsSameUrlForSameEntityTest()
        {
            ReInit();
            var urlService = Kernel.Get<DefaultTinyUrlService>();
            var entity = new EntityKeyWithTinyUrl() { Id = "TestId" };
            entity.TinyUrl = urlService.UrlFor(entity);
            Assert.IsNotNullOrEmpty(entity.TinyUrl);
            StoreEntity(entity);


            var sameUrl = urlService.UrlFor(entity);
            Assert.IsNotNullOrEmpty(sameUrl);

            Assert.AreEqual(entity.TinyUrl, sameUrl);
        }

        [Test]
        public void DefaultTinyUrlServiceEntityInfoForReturnsInfoForEntityWithTinyUrl()
        {
            ReInit();
            var urlService = Kernel.Get<DefaultTinyUrlService>();
            var entity = new EntityKeyWithTinyUrl() { Id = "TestId" };
            entity.TinyUrl = urlService.UrlFor(entity);
            StoreEntity(entity);

            Assert.IsNotNullOrEmpty(entity.TinyUrl);

            var ret = urlService.EntityInfoFor(entity.TinyUrl);

            Assert.IsNotNull(ret);
            
            Assert.AreEqual(entity.Id, ret.Id);
            Assert.AreEqual(entity.PrimaryInterface, ret.PrimaryInterface);
        }

        private void StoreEntity(EntityKeyWithTinyUrl entity)
        {
            var repo = Kernel.Get<GenericRepositoryInterface>();
            using (var uow = Kernel.Get<UnitOfWorkFactoryInterface>().GetUnitOfWork(new[] {repo}))
            {
                repo.Store(entity);
            }
        }
    }
}
