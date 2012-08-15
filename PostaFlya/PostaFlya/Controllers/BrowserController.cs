using System.Web.Mvc;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Domain.Browser.Command;
using PostaFlya.Domain.Browser.Query;
using PostaFlya.Domain.Location;
using WebSite.Infrastructure.Command;
using PostaFlya.Models.Tags;

namespace PostaFlya.Controllers
{
    public class BrowserController : Controller
    {
        private readonly CommandBusInterface _commandBus;
        private readonly BrowserInformationInterface _browserInformation;
        private readonly BrowserQueryServiceInterface _browserQueryService;

        public BrowserController(CommandBusInterface commandBus, 
            BrowserInformationInterface browserInformation, BrowserQueryServiceInterface browserQueryService)
        {
            _commandBus = commandBus;
            _browserInformation = browserInformation;
            _browserQueryService = browserQueryService;
        }

        [HttpPost]
        public ActionResult AddTags(AddTagsModel tag)
        {
            var command = new AddTagCommand()
                              {
                                  BrowserId = _browserInformation.Browser.Id,
                                  Tags = tag.Tags
                              };

            _commandBus.Send(command);

            _browserInformation.Browser = _browserQueryService.FindById(_browserInformation.Browser.Id);

            return RedirectToAction("AddTags");
        }



        [HttpPost]
        public ActionResult SetDistance(int distance)
        {
            var command = new SetDistanceCommand()
                              {
                                  BrowserId = _browserInformation.Browser.Id,
                                  Distance = distance
                              };
            _commandBus.Send(command);

            _browserInformation.Browser = _browserQueryService.FindById(_browserInformation.Browser.Id);

            return View();
        }

        [HttpGet]
        public ActionResult SetLocation()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SetLocation(Location loation)
        {
            var command = new SetLocationCommand()
            {
                BrowserId = _browserInformation.Browser.Id,
                Location = loation
            };
            _commandBus.Send(command);

            _browserInformation.Browser = _browserQueryService.FindById(_browserInformation.Browser.Id);

            return View();
        }

    }
}