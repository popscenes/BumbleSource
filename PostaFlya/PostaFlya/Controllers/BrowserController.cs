using System.Web.Mvc;
using Website.Infrastructure.Command;
using PostaFlya.Models.Tags;
using Website.Application.Domain.Browser;
using Website.Domain.Browser;
using Website.Domain.Browser.Command;
using Website.Domain.Browser.Query;
using Website.Domain.Location;

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

            _browserInformation.Browser = _browserQueryService.FindById<Browser>(_browserInformation.Browser.Id);

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

            _browserInformation.Browser = _browserQueryService.FindById<Browser>(_browserInformation.Browser.Id);

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

            _browserInformation.Browser = _browserQueryService.FindById<Browser>(_browserInformation.Browser.Id);

            return View();
        }

    }
}