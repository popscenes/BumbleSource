using System;
using System.Collections.Generic;
using MbUnit.Framework;
using Ninject;
using Ninject.MockingKernel.Moq;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.TaskJob;
using PostaFlya.Domain.TaskJob.Command;
using PostaFlya.Domain.TaskJob.Query;
using Website.Infrastructure.Command;
using Website.Domain.Location;

namespace PostaFlya.Mocks.Domain.Data.Behaviour
{
    public static class TaskJobTestData
    {
        public static void AssertStoreRetrieve(TaskJobFlierBehaviourInterface storedTaskJob, TaskJobFlierBehaviourInterface retrievedTaskJob)
        {
            Assert.AreEqual(storedTaskJob.Id, retrievedTaskJob.Id);
            Assert.AreEqual(storedTaskJob.MaxAmount, retrievedTaskJob.MaxAmount);
            Assert.AreEqual(storedTaskJob.CostOverhead, retrievedTaskJob.CostOverhead);
            Assert.AreElementsEqualIgnoringOrder(storedTaskJob.ExtraLocations, retrievedTaskJob.ExtraLocations);
        }

        internal static TaskJobFlierBehaviourInterface AssertGetById(TaskJobFlierBehaviourInterface taskJob, TaskJobQueryServiceInterface queryService)
        {
            var retrievedFlier = queryService.FindById<TaskJobFlierBehaviour>(taskJob.Id);
            AssertStoreRetrieve(taskJob, retrievedFlier);

            return retrievedFlier;
        }


        internal static TaskJobFlierBehaviourInterface StoreOne(TaskJobFlierBehaviourInterface taskJob, TaskJobRepositoryInterface repository, StandardKernel kernel)
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

        internal static void UpdateOne(TaskJobFlierBehaviourInterface taskJob, TaskJobRepositoryInterface repository, StandardKernel kernel)
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

        public static TaskJobBidInterface GetBid(TaskJobFlierBehaviourInterface flierBehaviour)
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
