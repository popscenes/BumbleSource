using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ninject;
using Website.Azure.Common.Environment;
using PostaFlya.DataRepository.Behaviour.TaskJob;
using PostaFlya.Domain.TaskJob;
using PostaFlya.Domain.TaskJob.Command;
using PostaFlya.Domain.TaskJob.Query;
using Website.Infrastructure.Command;
using Website.Domain.Location;

namespace PostaFlya.DataRepository.Tests.Behaviour.TaskJob
{
    [TestFixture("dev")]
    [TestFixture("real")]
    public class TaskJobRepositoryTests
    {

        StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }


        public TaskJobRepositoryTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        } 

        TaskJobRepositoryInterface _taskJobRepository = null;


        private TaskJobQueryServiceInterface _taskJobQueryService;

        public static void BindTaskJobRepository(StandardKernel kernel)
        {
//            kernel.Bind<TableNameAndPartitionProviderInterface>()
//            .ToConstant(new TableNameAndPartitionProvider<TaskJobFlierBehaviourInterface>()
//                                {
//                                    {typeof (TaskJobTableEntry), 0, "taskJobTest", t => t.Id, t => t.Id}
//                                })
//            .WhenAnyAnchestorNamed("taskjob")
//            .InSingletonScope();
//
//            var context = kernel.Get<AzureTableContext>("taskjob");
//            context.InitFirstTimeUse();
//            context.Delete<TaskJobTableEntry>(null, 0);
//            context.SaveChanges();
        }

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {

            BindTaskJobRepository(Kernel);

            _taskJobRepository = Kernel.Get<TaskJobRepositoryInterface>();
            _taskJobQueryService = Kernel.Get<TaskJobQueryServiceInterface>();
        }


        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
           // Kernel.Unbind<TableNameAndPartitionProviderInterface>();
            AzureEnv.UseRealStorage = false;
        }

        
        [Test]
        public void TestCreateTaskJobRepository()
        {
            var repository = Kernel.Get<TaskJobRepositoryInterface>();
            Assert.IsNotNull(repository);
            Assert.That(repository, Is.InstanceOf<AzureTaskJobRepository>());

            var queryService = Kernel.Get<TaskJobQueryServiceInterface>();
            Assert.IsNotNull(queryService);
            Assert.That(queryService, Is.InstanceOf<AzureTaskJobRepository>());
        }

        [Test]
        public void StoreTaskJobRepositoryTest()
        {
            StoreTaskJobRepository();
        }

        public TaskJobFlierBehaviourInterface StoreTaskJobRepository()
        {

            var flierId = Guid.NewGuid().ToString();
            var browsId = Guid.NewGuid().ToString();
            var task = new TaskJobFlierBehaviour()
            {
                Flier = new Domain.Flier.Flier(){Id = flierId},
                Id = flierId,
                MaxAmount = 100,
                CostOverhead = 10,
                ExtraLocations = new Locations(){new Location(20,20), new Location(21,21)}
            };

            Store(task);

            return task;
        }

        [Test]
        public void QueryTaskJobRepositoryTest()
        {
            QueryTaskJobRepository();
        }

        public TaskJobFlierBehaviourInterface QueryTaskJobRepository()
        {
            var task = StoreTaskJobRepository();
            return Query(task);
        }

        [Test]
        public void TestQueryModifySaveTaskJobRepository()
        {
            var task = QueryTaskJobRepository();
            
            task.CostOverhead = 11;
            task.ExtraLocations.Add(new Location(21, 21));
            task.MaxAmount = 200;

            Update(task);
            Query(task);
        }

        private void Store(TaskJobFlierBehaviourInterface source)
        {
            var uow = Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() {_taskJobRepository});
            using (uow)
            {

                _taskJobRepository.Store(source);

            }

            Assert.IsTrue(uow.Successful);
        }

        private void Update(TaskJobFlierBehaviourInterface source)
        {
            using (Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { _taskJobRepository }))
            {
                _taskJobRepository.UpdateEntity<TaskJobFlierBehaviour>(source.Id, e => e.CopyFieldsFrom(source));
            }
        }

        private TaskJobFlierBehaviourInterface Query(TaskJobFlierBehaviourInterface source)
        {
            var storedbyid = _taskJobQueryService.FindById<TaskJobFlierBehaviour>(source.Id);

            AssertAreEqual(source, storedbyid);

            return storedbyid;
        }

        private static void AssertAreEqual(TaskJobFlierBehaviourInterface source, TaskJobFlierBehaviourInterface query)
        {
            Assert.IsNotNull(query);
            Assert.AreEqual(source.Id, query.Id);
            Assert.AreEqual(source.MaxAmount, query.MaxAmount);
            Assert.AreEqual(source.CostOverhead, query.CostOverhead);
            CollectionAssert.AreEquivalent(source.ExtraLocations, query.ExtraLocations);
        }

    }
}
