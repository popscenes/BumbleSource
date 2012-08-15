using System.Diagnostics;

namespace WebSite.Infrastructure.Command
{
    public class DefaultCommandBus : CommandBusInterface
    {
        private readonly CommandHandlerRespositoryInterface _handlerRespository;
        public DefaultCommandBus(CommandHandlerRespositoryInterface handlerRespository)
        {
            _handlerRespository = handlerRespository;
        }

        public object Send<TCommand>(TCommand command) where TCommand : class, CommandInterface
        {
            var handler = _handlerRespository.findHandler(command);

//            var serializer = new JavaScriptSerializer();
//            ViewBag.BrowserInfoJson = serializer.Serialize(_browserQueryService.ToCurrentBrowserModel(_blobStorage));
            return handler.Handle(command);
        }
    }
}