﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using Website.Application.Binding;
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

        readonly TestQueue _testQueue = new TestQueue();
        readonly HashSet<TinyUrlRecordInterface> _store = RepoCoreUtil.GetMockStore<TinyUrlRecordInterface>();
        
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {           
            RepoCoreUtil.SetupRepo<GenericRepositoryInterface, TinyUrlRecord, TinyUrlRecordInterface, TinyUrlRecordInterface>(_store, Kernel, TinyUrlRecordInterfaceExtensions.CopyFieldsFrom);
            RepoCoreUtil.SetupQueryService<GenericQueryServiceInterface, TinyUrlRecord, TinyUrlRecordInterface, TinyUrlRecordInterface>(_store, Kernel, TinyUrlRecordInterfaceExtensions.CopyFieldsFrom);
            RepoCoreUtil.FindAggregateEntities<GenericQueryServiceInterface, TinyUrlRecord, TinyUrlRecordInterface>(_store, Kernel, TinyUrlRecordInterfaceExtensions.CopyFieldsFrom);
            Kernel.Bind<QueueInterface>()
              .ToConstant(_testQueue)
              .WhenTargetHas<TinyUrlQueue>();
            ReInit();
        }
 
        private  void ReInit()
        {
            _store.Clear();
            _testQueue.Clear();
            _testQueue.AddMessage(new QueueMessage(){Bytes = System.Text.Encoding.ASCII.GetBytes("http://tin.y/1")});
            _testQueue.AddMessage(new QueueMessage() { Bytes = System.Text.Encoding.ASCII.GetBytes("http://tin.y/2") });
            _testQueue.AddMessage(new QueueMessage() { Bytes = System.Text.Encoding.ASCII.GetBytes("http://tin.y/3") });
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<QueueInterface>();
        }

        class  TestEntity : EntityBase<TestEntity>, EntityInterface, TinyUrlInterface
        {
            public string TinyUrl { get; set; }
        }

        [Test]
        public void DefaultTinyUrlServiceUrlForReturnsANewUrlForANewEntityTest()
        {
            ReInit();
            var urlService = Kernel.Get<DefaultTinyUrlService>();
            var entity = new TestEntity() {Id = "TestId"};
            entity.TinyUrl = urlService.UrlFor(entity);
            Assert.IsNotNullOrEmpty(entity.TinyUrl);

            var entityTwo = new TestEntity() { Id = "TestIdTwo" };
            entityTwo.TinyUrl = urlService.UrlFor(entityTwo);
            Assert.IsNotNullOrEmpty(entityTwo.TinyUrl);

            Assert.AreNotEqual(entity.TinyUrl, entityTwo.TinyUrl);
        }

        [Test]
        public void DefaultTinyUrlServiceUrlForReturnsSameUrlForSameEntityTest()
        {
            ReInit();
            var urlService = Kernel.Get<DefaultTinyUrlService>();
            var entity = new TestEntity() { Id = "TestId" };
            entity.TinyUrl = urlService.UrlFor(entity);
            Assert.IsNotNullOrEmpty(entity.TinyUrl);

            var sameUrl = urlService.UrlFor(entity);
            Assert.IsNotNullOrEmpty(sameUrl);

            Assert.AreEqual(entity.TinyUrl, sameUrl);
        }

        [Test]
        public void DefaultTinyUrlServiceEntityInfoForReturnsInfoForEntityWithTinyUrl()
        {
            ReInit();
            var urlService = Kernel.Get<DefaultTinyUrlService>();
            var entity = new TestEntity() { Id = "TestId" };
            entity.TinyUrl = urlService.UrlFor(entity);
            Assert.IsNotNullOrEmpty(entity.TinyUrl);

            var ret = urlService.EntityInfoFor(entity.TinyUrl);

            Assert.IsNotNull(ret);
            
            Assert.AreEqual(entity.Id, ret.Id);
            Assert.AreEqual(entity.PrimaryInterface, ret.PrimaryInterface);
        }
    }
}
