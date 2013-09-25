using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ninject;
using Website.Azure.Common.Environment;
using Website.Azure.Common.TableStorage;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace Website.Azure.Common.Tests.TableStorage
{

    class JsonTestEntity
    {
        public string Prop { get; set; }
        public string PropTwo { get; set; }
        public string PropThree { get; set; }
        public HashSet<string> HashTest { get; set; }
        public List<TwoEntity> SubEntity { get; set; }
    }

    public class JsonTestConcurrentEntity : EntityBase<EntityInterface>, AggregateRootInterface, EntityInterface
    {
        public string Prop { get; set; }
        public string PropTwo { get; set; }
        public int Counter { get; set; }
        public string ConcurrentOne { get; set; }
        public string ConcurrentTwo { get; set; }
    }


    [TestFixture("dev")]
    [TestFixture("real")]
    public class JsonTableEntryTests
    {
        static StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        public JsonTableEntryTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Kernel.Rebind<TableContextInterface>()
                .To<TableContext>().InTransientScope();
            Kernel.Rebind<JsonRepository>()
                .ToSelf().InTransientScope();

            Kernel.Rebind<TableNameAndIndexProviderServiceInterface>()
                .To<TableNameAndIndexProviderService>()
                .InSingletonScope();

            var tableNameAndPartitionProviderService = Kernel.Get<TableNameAndIndexProviderServiceInterface>();
            tableNameAndPartitionProviderService.Add<JsonTestEntity>("testJsonEntity", entity => entity.Prop, entity => entity.PropTwo);

            tableNameAndPartitionProviderService.Add<JsonTestConcurrentEntity>("testJsonConcurrEntity", entity => entity.Id);

            var context = Kernel.Get<TableContextInterface>();

            foreach (var tableName in tableNameAndPartitionProviderService.GetAllTableNames())
            {
                context.InitTable<JsonTableEntry>(tableName);
            }

            context.Delete<JsonTableEntry>("testJsonEntity", null);
            context.Delete<JsonTableEntry>("testJsonConcurrEntity", null);

            context.SaveChanges();
        }


        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<TableNameAndIndexProviderServiceInterface>();
            Kernel.Unbind<TableContextInterface>();
            AzureEnv.UseRealStorage = false;
        }

        internal static void AssertAreEqual(JsonTestEntity one, JsonTestEntity two)
        {
            Assert.AreEqual(one.Prop, two.Prop);
            Assert.AreEqual(one.PropTwo, two.PropTwo);
            Assert.AreEqual(one.PropThree, two.PropThree);

            for (var i = 0; i < one.SubEntity.Count; i++)
            {
                TwoEntity.AssertAreEqual(one.SubEntity[i], two.SubEntity[i]);
            }

            CollectionAssert.AreEqual(one.HashTest, two.HashTest);
        }

        [Test]
        public void JsonTableEntryTestStoreRetrieve()
        {
            var testob = new JsonTestEntity()
                             {
                                 Prop = "Hello",
                                 PropTwo = "There",
                                 PropThree = "Yo",
                                 HashTest = new HashSet<string>(){"one", "two"},
                                 SubEntity = new List<TwoEntity>(){new TwoEntity(){Prop = "TwoProp",PropTwo = null}}
                             };

            var nameAndPartitionProviderService = Kernel.Get<TableNameAndIndexProviderServiceInterface>();
            var partKeyFunc = nameAndPartitionProviderService.GetPartitionKeyFunc<JsonTestEntity>();
            var rowKeyFunc = nameAndPartitionProviderService.GetRowKeyFunc<JsonTestEntity>();


            var tableEntry = new JsonTableEntry()
                                 {
                                     KeyChanged = false,
                                     PartitionKey = partKeyFunc(testob),
                                     RowKey = rowKeyFunc(testob)
                                 };
            tableEntry.Init(testob);
            tableEntry.UpdateEntry();

            var tableName = nameAndPartitionProviderService.GetTableName<JsonTestEntity>();
            var tabCtx = Kernel.Get<TableContextInterface>();
            tabCtx.Store(tableName, tableEntry);
            tabCtx.SaveChanges();

            var tabCtxRet = Kernel.Get<TableContextInterface>();
            var ret = tabCtxRet.PerformQuery<JsonTableEntry>(tableName, query: e => e.PartitionKey == tableEntry.PartitionKey 
                                                                                    && e.RowKey == tableEntry.RowKey)
                                                                   .SingleOrDefault();

            Assert.IsNotNull(ret);
            var deserialized = ret.GetEntity<JsonTestEntity>();
            AssertAreEqual(testob, deserialized);
        }

        [Test]
        public void JsonTableEntryTestStoreRetrieveUpdate()
        {
            var testob = new JsonTestEntity()
            {
                Prop = "Hello123",
                PropTwo = "There123",
                PropThree = "Yo",
                HashTest = new HashSet<string>() { "one", "two" },
                SubEntity = new List<TwoEntity>() { new TwoEntity() { Prop = "TwoProp", PropTwo = null } }
            };

            var nameAndPartitionProviderService = Kernel.Get<TableNameAndIndexProviderServiceInterface>();
            var partKeyFunc = nameAndPartitionProviderService.GetPartitionKeyFunc<JsonTestEntity>();
            var rowKeyFunc = nameAndPartitionProviderService.GetRowKeyFunc<JsonTestEntity>();


            var tableEntry = new JsonTableEntry()
            {
                KeyChanged = false,
                PartitionKey = partKeyFunc(testob),
                RowKey = rowKeyFunc(testob)
            };
            tableEntry.Init(testob);
            tableEntry.UpdateEntry();

            var tableName = nameAndPartitionProviderService.GetTableName<JsonTestEntity>();
            var tabCtx = Kernel.Get<TableContextInterface>();
            tabCtx.Store(tableName, tableEntry);
            tabCtx.SaveChanges();

            var tabCtxRet = Kernel.Get<TableContextInterface>();
            var ret = tabCtxRet.PerformQuery<JsonTableEntry>(tableName, query: e => e.PartitionKey == tableEntry.PartitionKey
                                                                                    && e.RowKey == tableEntry.RowKey)
                                                                   .SingleOrDefault();

            Assert.IsNotNull(ret);
            var deserialized = ret.GetEntity<JsonTestEntity>();
            AssertAreEqual(testob, deserialized);

            //get a new context and find the same entity
            var tabCtxTwo = Kernel.Get<TableContextInterface>();
            ret = tabCtxTwo.PerformQuery<JsonTableEntry>(tableName, query: e => e.PartitionKey == tableEntry.PartitionKey
                                                                                && e.RowKey == tableEntry.RowKey)
                                                       .SingleOrDefault();
            deserialized = ret.GetEntity<JsonTestEntity>();
            AssertAreEqual(testob, deserialized);
            deserialized.PropThree = "MODIFIED";
            ret.UpdateEntry();
            //update in next context
            tabCtxTwo.Store(tableName, ret);
            tabCtxTwo.SaveChanges();

            //use initial context to retrieve entity
            var reAgain = tabCtxRet.PerformQuery<JsonTableEntry>(tableName, query: e => e.PartitionKey == tableEntry.PartitionKey
                                                                                        && e.RowKey == tableEntry.RowKey)
                                                                   .SingleOrDefault();
            Assert.IsNotNull(reAgain);
            var deserializedAgain = reAgain.GetEntity<JsonTestEntity>();
            AssertAreEqual(deserialized, deserializedAgain);

        }

        [Test]
        public void JsonTableEntryTestStoreLargeEntity()
        {
//            if (AzureEnv.UseRealStorage)//will take too long to run
//                return;

            int numChars = 1024 * 128;//256k utf16
            var strBld = new StringBuilder(numChars);
            strBld.Append('a', numChars);

            var testob = new JsonTestEntity()
            {
                Prop = "Hello1",
                PropTwo = "There2",
                PropThree = strBld.ToString(),
                HashTest = new HashSet<string>() { "one", "two" },
                SubEntity = new List<TwoEntity>() { new TwoEntity() { Prop = "TwoProp", PropTwo = null } }
            };



            var nameAndPartitionProviderService = Kernel.Get<TableNameAndIndexProviderServiceInterface>();
            var partKeyFunc = nameAndPartitionProviderService.GetPartitionKeyFunc<JsonTestEntity>();
            var rowKeyFunc = nameAndPartitionProviderService.GetRowKeyFunc<JsonTestEntity>();


            var tableEntry = new JsonTableEntry()
            {
                KeyChanged = false,
                PartitionKey = partKeyFunc(testob),
                RowKey = rowKeyFunc(testob)
            };
            tableEntry.Init(testob);
            tableEntry.UpdateEntry();

            var tableName = nameAndPartitionProviderService.GetTableName<JsonTestEntity>();
            var tabCtx = Kernel.Get<TableContextInterface>();
            tabCtx.Store(tableName, tableEntry);
            tabCtx.SaveChanges();

            var tabCtxRet = Kernel.Get<TableContextInterface>();
            var ret = tabCtxRet.PerformQuery<JsonTableEntry>(tableName, query: e => e.PartitionKey == tableEntry.PartitionKey
                                                                                    && e.RowKey == tableEntry.RowKey)
                                                                   .SingleOrDefault();

            Assert.IsNotNull(ret);
            var deserialized = ret.GetEntity<JsonTestEntity>();
            AssertAreEqual(testob, deserialized);
        }

        [Test]
        public void MultipleUpdatesInUowAreAppliedSuccessfully()
        {
            var testTwo = new JsonTestConcurrentEntity()
            {
                Prop = "123",
                Id = Guid.NewGuid().ToString(),
                Counter = 0
            };

            var uow = Kernel.Get<UnitOfWorkInterface>().Begin();
            using (uow)
            {
                var repo1 = Kernel.Get<GenericRepositoryInterface>();
                repo1.Store(testTwo);
            }

            Assert.IsTrue(uow.Successful);


            uow = Kernel.Get<UnitOfWorkInterface>().Begin();
            using (uow)
            {
                var repo1 = Kernel.Get<GenericRepositoryInterface>();

                repo1.UpdateEntity<JsonTestConcurrentEntity>(testTwo.Id
                    , flier =>
                    {
                        flier.ConcurrentTwo = "TwoChanged";
                    });

                repo1.UpdateEntity<JsonTestConcurrentEntity>(testTwo.Id
                    , flier =>
                    {
                        flier.ConcurrentOne = "OneChanged";
                    });
            }

            Assert.IsTrue(uow.Successful);

            uow = Kernel.Get<UnitOfWorkInterface>().Begin();
            using (uow)
            {
                var query = Kernel.Get<GenericQueryServiceInterface>();

                var retEntity = query.FindById<JsonTestConcurrentEntity>(testTwo.Id);
                Assert.AreEqual("OneChanged", retEntity.ConcurrentOne);
                Assert.AreEqual("TwoChanged", retEntity.ConcurrentTwo);
            }
        }

        [Test]
        public void AzureRepositoryRetriesUpdateIfConcurrencyExceptionOccurs()
        {
            var testTwo = new JsonTestConcurrentEntity()
            {
                Prop = "123",
                Id = Guid.NewGuid().ToString(),
                Counter = 0
            };

            var uow = Kernel.Get<UnitOfWorkInterface>().Begin();
            using (uow)
            {
                var repo1 = Kernel.Get<GenericRepositoryInterface>();
                repo1.Store(testTwo);          
            }

            Assert.IsTrue(uow.Successful);

            var tryCount = 0;
            UnitOfWorkInterface unitOfWork;

            uow = Kernel.Get<UnitOfWorkInterface>().Begin();
            using (uow)
            {
                var repo1 = Kernel.Get<GenericRepositoryInterface>();

                repo1.UpdateEntity<JsonTestConcurrentEntity>(testTwo.Id
                    , flier =>
                    {
                        if (tryCount++ == 0)
                        {
                            var otherrepo = Kernel.Get<JsonRepository>();
                            otherrepo.UpdateEntity<JsonTestConcurrentEntity>(testTwo.Id, f
                                                                                         =>
                                {
                                    f.ConcurrentOne = "OneChanged";
                                    f.Counter++;
                                });
                            otherrepo.SaveChanges();
                        }

                        flier.Counter++;
                        flier.ConcurrentTwo = "TwoChanged";

                    }

                    );
            }

            Assert.IsTrue(uow.Successful);

            uow = Kernel.Get<UnitOfWorkInterface>().Begin();
            using (uow)
            {
                var query = Kernel.Get<GenericQueryServiceInterface>();

                var retEntity = query.FindById<JsonTestConcurrentEntity>(testTwo.Id);
                Assert.AreEqual(2, retEntity.Counter);
                Assert.AreEqual("OneChanged", retEntity.ConcurrentOne);
                Assert.AreEqual("TwoChanged", retEntity.ConcurrentTwo);
            }


        }

        [Test]
        public void AzureRepositoryConcurrencyRetryShouldReloadCurrentState()
        {
            var testTwo = new JsonTestConcurrentEntity()
            {
                Prop = "123",
                Id = Guid.NewGuid().ToString(),
                ConcurrentTwo = "Initial",
                Counter = 0
            };

            var uow = Kernel.Get<UnitOfWorkInterface>().Begin();
            using (uow)
            {
                var repo1 = Kernel.Get<GenericRepositoryInterface>();
                repo1.Store(testTwo);
            }

            Assert.IsTrue(uow.Successful);

            var tryCount = 0;
            UnitOfWorkInterface unitOfWork;

            uow = Kernel.Get<UnitOfWorkInterface>().Begin();
            using (uow)
            {
                var repo1 = Kernel.Get<GenericRepositoryInterface>();

                //attempt one update action which will cause retry
                repo1.UpdateEntity<JsonTestConcurrentEntity>(testTwo.Id
                    , flier =>
                    {
                        if (tryCount++ == 0)
                        {
                            var otherrepo = Kernel.Get<JsonRepository>();
                            otherrepo.UpdateEntity<JsonTestConcurrentEntity>(testTwo.Id, f
                            =>
                            {
                                f.ConcurrentOne = "OneChanged";
                                f.ConcurrentTwo = "TwoChanged";
                            });
                            otherrepo.SaveChanges();
                        }

                        flier.ConcurrentTwo = flier.ConcurrentTwo + "ChangedAgain";

                    }
                );
                //in the same unit of work attempt to modify a value we have
                //already modified in a retry. ther result should be as all actions have taken 
                //place sequentially ie "TwoChanged" -> "TwoChanged" + "ChangedAgain" -> TwoChanged" + "ChangedAgain" + "Again"
                repo1.UpdateEntity<JsonTestConcurrentEntity>(testTwo.Id
                    , flier =>
                    {
                        flier.ConcurrentTwo = flier.ConcurrentTwo + "Again";
                    }
                );
            }

            Assert.IsTrue(uow.Successful);

            uow = Kernel.Get<UnitOfWorkInterface>().Begin();
            using (uow)
            {
                var query = Kernel.Get<GenericQueryServiceInterface>();

                var retEntity = query.FindById<JsonTestConcurrentEntity>(testTwo.Id);
                Assert.AreEqual("OneChanged", retEntity.ConcurrentOne);
                Assert.AreEqual("TwoChangedChangedAgainAgain", retEntity.ConcurrentTwo);
            }


        }
    }
}
