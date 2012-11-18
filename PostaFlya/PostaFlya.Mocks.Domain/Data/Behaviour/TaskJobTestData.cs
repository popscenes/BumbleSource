using System;
using System.Collections.Generic;
using NUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.TaskJob;
using Website.Infrastructure.Command;
using Website.Domain.Location;
using Website.Infrastructure.Query;

namespace PostaFlya.Mocks.Domain.Data.Behaviour
{
    public static class TaskJobTestData
    {
        public static void AssertStoreRetrieve(TaskJobFlierBehaviourInterface storedTaskJob, TaskJobFlierBehaviourInterface retrievedTaskJob)
        {
            Assert.AreEqual(storedTaskJob.Id, retrievedTaskJob.Id);
            Assert.AreEqual(storedTaskJob.MaxAmount, retrievedTaskJob.MaxAmount);
            Assert.AreEqual(storedTaskJob.CostOverhead, retrievedTaskJob.CostOverhead);
            CollectionAssert.AreEquivalent(storedTaskJob.ExtraLocations, retrievedTaskJob.ExtraLocations);
        }

        internal static TaskJobFlierBehaviourInterface AssertGetById(TaskJobFlierBehaviourInterface taskJob, GenericQueryServiceInterface queryService)
        {
            var retrievedFlier = queryService.FindById<TaskJobFlierBehaviour>(taskJob.Id);
            AssertStoreRetrieve(taskJob, retrievedFlier);

            return retrievedFlier;
        }


        internal static TaskJobFlierBehaviourInterface StoreOne(TaskJobFlierBehaviourInterface taskJob, GenericRepositoryInterface repository, StandardKernel kernel)
        {
            var uow = kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() {repository});
            using (uow)
            {

                repository.Store(taskJob);
            }

            Assert.IsTrue(uow.Successful);
            return taskJob;
        }

        internal static void UpdateOne(TaskJobFlierBehaviourInterface taskJob, GenericRepositoryInterface repository, StandardKernel kernel)
        {
            using (kernel.Get<UnitOfWorkFactoryInterface>()
                .GetUnitOfWork(new List<RepositoryInterface>() { repository }))
            {
                repository.UpdateEntity<TaskJobFlierBehaviour>(taskJob.Id, e => e.CopyFieldsFrom(taskJob));
            }
        }

        public static TaskJobFlierBehaviourInterface GetOne(MoqMockingKernel kernel)
        {
            var flier = FlierTestData.GetOne(kernel);
            flier.FlierBehaviour = FlierBehaviour.TaskJob;
            var ret = FlierTestData.GetBehaviour(kernel, flier) as TaskJobFlierBehaviourInterface;
            ret.ExtraLocations = new Locations(){new Location(234,234), new Location(123,123)};
            ret.CostOverhead = 111;
            ret.MaxAmount = 345;
            ret.Id = Guid.NewGuid().ToString();
            return ret;
        }

        public static TaskJobBid GetBid(TaskJobFlierBehaviourInterface flierBehaviour)
        {
            var rnd = new Random();
            return new TaskJobBid()
                       {
                           BrowserId = Guid.NewGuid().ToString(),
                           Id = Guid.NewGuid().ToString(),
                           TaskJobId = flierBehaviour.Id,
                           BidAmount = rnd.Next(300, 700)/10.0
                       };
        }
    }
}
