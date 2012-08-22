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

    internal class AzureTaskJobRepository : JsonRepository
        , TaskJobRepositoryInterface
        , TaskJobQueryServiceInterface
    {

        public AzureTaskJobRepository(TableContextInterface tableContext
            , TableNameAndPartitionProviderServiceInterface nameAndPartitionProviderService) 
            : base(tableContext, nameAndPartitionProviderService)
        {
        }

        public IQueryable<TaskJobBidInterface> GetBids(string taskJobId)
        {
            throw new NotImplementedException();
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
