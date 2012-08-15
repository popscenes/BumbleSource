using System;
using System.Collections.Generic;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Microsoft.WindowsAzure.StorageClient;
using Ninject;
using WebSite.Azure.Common.Environment;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.DataRepository.Behaviour.TaskJob;
using PostaFlya.Domain.Content.Query;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.TaskJob;
using PostaFlya.Domain.TaskJob.Command;
using PostaFlya.Domain.TaskJob.Query;
using WebSite.Infrastructure.Command;

namespace PostaFlya.DataRepository.Tests.Behaviour.TaskJob
{
    [TestFixture]
    public class TaskJobRepositoryTests
    {

        StandardKernel Kernel
        {
            get { return TestFixtureSetup.CurrIocKernel; }
        }

        [Row("dev")] 
        [Row("real")]
        public TaskJobRepositoryTests(string env)
        {
            AzureEnv.UseRealStorage = env == "real";
        } 

        TaskJobRepositoryInterface _taskJobRepository = null;


        private TaskJobQueryServiceInterface _taskJobQueryService;

        public static void BindTaskJobRepository(StandardKernel kernel)
        {
            kernel.Bind<TableNameAndPartitionProviderInterface>()
            .ToConstant(new TableNameAndPartitionProvider<TaskJobFlierBehaviourInterface>()
                                {
                                    {typeof (TaskJobTableEntry), 0, "taskJobTest", t => t.Id, t => t.Id}
                                })
            .WhenAnyAnchestorNamed("taskjob")
            .InSingletonScope();

            var context = kernel.Get<AzureTableContext>("taskjob");
            context.InitFirstTimeUse();
            context.Delete<TaskJobTableEntry>(null, 0);
            context.SaveChanges();
        }

        [FixtureSetUp]
        public void FixtureSetUp()
        {

            BindTaskJobRepository(Kernel);

            _taskJobRepository = Kernel.Get<TaskJobRepositoryInterface>();
            _taskJobQueryService = Kernel.Get<TaskJobQueryServiceInterface>();
        }


        [FixtureTearDown]
        public void FixtureTearDown()
        {
            Kernel.Unbind<TableNameAndPartitionProviderInterface>();
            AzureEnv.UseRealStorage = false;
        }

        
        [Test]
        public void TestCreateTaskJobRepository()
        {
            var repository = Kernel.Get<TaskJobRepositoryInterface>();
            Assert.IsNotNull(repository);
            Assert.IsInstanceOfType<AzureTaskJobRepository>(repository);

            var queryService = Kernel.Get<TaskJobQueryServiceInterface>();
            Assert.IsNotNull(queryService);
            Assert.IsInstanceOfType<AzureTaskJobRepository>(queryService);
        }

        [Test]
        public TaskJobFlierBehaviourInterface TestStoreTaskJobRepository()
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
        public TaskJobFlierBehaviourInterface TestQueryTaskJobRepository()
        {
            var task = TestStoreTaskJobRepository();
            return Query(task);
        }

        [Test]
        public void TestQueryModifySaveTaskJobRepository()
        {
            var task = TestQueryTaskJobRepository();
            
            task.CostOverhead = 11;
            task.ExtraLocations.Add(new Location(21, 21));
            task.MaxAmount = 200;

            Update(task);
            Query(task);
        }

        private void Store(TaskJobFlierBehaviourInterface source)
        {
            using (Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { _taskJobRepository }))
            {

                _taskJobRepository.Store(source);

            }
        }

        private void Update(TaskJobFlierBehaviourInterface source)
        {
            using (Kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { _taskJobRepository }))
            {
                _taskJobRepository.UpdateEntity(source.Id, e => e.CopyFieldsFrom(source));
            }
        }

        private TaskJobFlierBehaviourInterface Query(TaskJobFlierBehaviourInterface source)
        {
            var storedbyid = _taskJobQueryService.FindById(source.Id);

            AssertAreEqual(source, storedbyid);

            return storedbyid;
        }

        private static void AssertAreEqual(TaskJobFlierBehaviourInterface source, TaskJobFlierBehaviourInterface query)
        {
            Assert.IsNotNull(query);
            Assert.AreEqual(source.Id, query.Id);
            Assert.AreEqual(source.MaxAmount, query.MaxAmount);
            Assert.AreEqual(source.CostOverhead, query.CostOverhead);
            Assert.AreElementsEqualIgnoringOrder(source.ExtraLocations, query.ExtraLocations);
        }

    }
}
