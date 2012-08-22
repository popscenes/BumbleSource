using System.Linq;
using System.Net.Http;
using System.Web.Http;
using WebSite.Application.Binding;
using PostaFlya.Application.Domain.Content;
using PostaFlya.Areas.TaskJob.Models;
using PostaFlya.Attributes;
using WebSite.Application.Content;
using WebSite.Common.Extension;
using PostaFlya.Controllers;
using PostaFlya.Domain.Behaviour;
using PostaFlya.Domain.Flier.Query;
using PostaFlya.Domain.Location;
using PostaFlya.Domain.TaskJob;
using PostaFlya.Domain.TaskJob.Command;
using PostaFlya.Domain.TaskJob.Query;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Areas.TaskJob.Controllers
{
    [BrowserAuthorize(Roles = "IdentityVerified")]
    public class MyTaskJobsController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly FlierQueryServiceInterface _flierQueryService;
        private readonly TaskJobQueryServiceInterface _jobQueryService;
        private readonly BlobStorageInterface _blobStorage;

        public MyTaskJobsController(CommandBusInterface commandBus, 
            FlierQueryServiceInterface flierQueryService
            , TaskJobQueryServiceInterface jobQueryService
            , [ImageStorage]BlobStorageInterface blobStorage)
        {
            _commandBus = commandBus;
            _flierQueryService = flierQueryService;
            _jobQueryService = jobQueryService;
            _blobStorage = blobStorage;
        }

        //re-introduce with a test based of a specification!
        //just put here to test api routing
        public IQueryable<MyTaskJobModel> Get(string browserId)
        {
           return _flierQueryService.GetByBrowserId<Domain.Flier.Flier>(browserId).Where(f => f.FlierBehaviour == FlierBehaviour.TaskJob)
                .Select(f => _jobQueryService.FindById<TaskJobFlierBehaviour>(f.Id)
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