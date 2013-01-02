using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Website.Application.Binding;
using PostaFlya.Areas.TaskJob.Models;
using Website.Application.Content;
using Website.Application.Domain.Browser.Web;
using Website.Common.Extension;
using PostaFlya.Controllers;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.TaskJob;
using PostaFlya.Domain.TaskJob.Command;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Command;
using Website.Domain.Location;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.TaskJob.Controllers
{
    [BrowserAuthorize(Roles = "IdentityVerified")]
    public class MyTaskJobsController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly QueryServiceForBrowserAggregateInterface _queryService;
        private readonly BlobStorageInterface _blobStorage;

        public MyTaskJobsController(CommandBusInterface commandBus,
            QueryServiceForBrowserAggregateInterface queryService
            , [ImageStorage]BlobStorageInterface blobStorage)
        {
            _commandBus = commandBus;
            _queryService = queryService;
            _blobStorage = blobStorage;
        }

        //re-introduce with a test based of a specification!
        //just put here to test api routing
        public IQueryable<MyTaskJobModel> Get(string browserId)
        {
           return _queryService.GetByBrowserId<Domain.Flier.Flier>(browserId).Where(f => f.FlierBehaviour == FlierBehaviour.TaskJob)
                .Select(f => _queryService.FindById<TaskJobFlierBehaviour>(f.Id)
                    .ToMyTaskJobModel(_blobStorage));
        }

        public HttpResponseMessage Post(string browserId, TaskJobBehaviourCreateModel createModel)
        {
            var res = _commandBus.Send(
                new TaskJobBehaviourCreateCommand()
                    {
                        BrowserId = browserId,
                        FlierId = createModel.FlierId,
                        MaxAmount = createModel.MaxAmount,
                        CostOverhead = createModel.CostOverhead,
                        ExtraLocations = new Locations(createModel.ExtraLocations.Select(l => l.ToDomainModel()))
                    }
                );
            return this.GetResponseForRes(res);
        }
    }
}