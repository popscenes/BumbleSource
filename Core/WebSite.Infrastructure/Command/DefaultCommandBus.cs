using System.Diagnostics;

namespace Website.Infrastructure.Command
{
    public class DefaultCommandBus : CommandBusInterface
    {
        private readonly CommandHandlerRespositoryInterface _handlerRespository;
        public DefaultCommandBus(CommandHandlerRespositoryInterface handlerRespository)
        {
            _handlerRespository = handlerRespository;
        }

        public object Send<CommandType>(CommandType command) where CommandType : class, CommandInterface
        {
            var handler = _handlerRespository.FindHandler(command);

//            var serializer = new JavaScriptSerializer();
//            ViewBag.BrowserInfoJson = serializer.Serialize(_browserQueryService.ToCurrentBrowserModel(_blobStorage));
            return handler.Handle(command);
        }
    }
}