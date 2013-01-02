using System.Net.Http;
using System.Web.Http;
using PostaFlya.Areas.TaskJob.Models;
using Website.Application.Domain.Browser.Web;
using Website.Common.Extension;
using PostaFlya.Domain.TaskJob.Command;
using Website.Infrastructure.Command;

namespace PostaFlya.Areas.TaskJob.Controllers
{
    [BrowserAuthorize(Roles = "IdentityVerified")]
    public class MyTaskBidsController : ApiController
    {
        private readonly CommandBusInterface _commandBus;

        public MyTaskBidsController(CommandBusInterface commandBus)
        {
            _commandBus = commandBus;
        }

        public HttpResponseMessage Post(string browserId, TaskJobBidModel model)
        {
            var createTaskJobBid = new CreateTaskJobBidCommand()
                                       {
                                           BrowserId = browserId,
                                           TaskJobId = model.TaskJobId,
                                           BidAmount = model.BidAmount
                                       };
            var res = _commandBus.Send(createTaskJobBid);
            return this.GetResponseForRes(res);
        }
    }
}