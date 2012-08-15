using System;
using System.Data.Services.Client;
using System.Linq;
using System.Text;
using Ninject;
using WebSite.Azure.Common.TableStorage;
using PostaFlya.Domain.Flier;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.TaskJob;
using PostaFlya.Domain.TaskJob.Command;
using PostaFlya.Domain.TaskJob.Query;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Query;
using WebSite.Infrastructure.Util;

namespace PostaFlya.DataRepository.Behaviour.TaskJob
{

    internal class AzureTaskJobRepository : AzureRepositoryBase<TaskJobFlierBehaviourInterface, TaskJobStorageDomain>
        , TaskJobRepositoryInterface
        , TaskJobQueryServiceInterface
    {
        private readonly AzureTableContext _tableContext;

        public AzureTaskJobRepository([Named("taskjob")]AzureTableContext taskJobTableContext)
            : base(taskJobTableContext)
        {
            _tableContext = taskJobTableContext;
        }

        public TaskJobFlierBehaviourInterface FindById(string id)
        {
            return TaskJobStorageDomain.FindById(id, _tableContext);
        }

        public IQueryable<TaskJobBidInterface> GetBids(string taskJobId)
        {
            throw new NotImplementedException();
        }


        protected override TaskJobStorageDomain GetEntityForUpdate(string id)
        {
            return TaskJobStorageDomain.GetEntityForUpdate(id, _tableContext);
        }

        protected override TaskJobStorageDomain GetStorageForEntity(TaskJobFlierBehaviourInterface entity)
        {
            return new TaskJobStorageDomain(entity, _tableContext);
        }

        object QueryServiceInterface.FindById(string id)
        {
            return FindById(id);
        }

        public bool BidOnTask(TaskJobBidInterface bid)
        {
            throw new NotImplementedException();
        }

        public TaskJobBidInterface GetBidForUpdate(string taskJobId, string browserId)
        {
            throw new NotImplementedException();
        }
    }
}
