using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostaFlya.Domain.Boards.Command;
using PostaFlya.Domain.Flier.Event;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Infrastructure.Publish;

namespace PostaFlya.Domain.Boards.Event
{
    public class MatchFlierToBoardEventHandler:
        HandleEventInterface<FlierModifiedEvent>
    {
        private readonly CommandBusInterface _commandBus;

        public MatchFlierToBoardEventHandler([WorkerCommandBus]CommandBusInterface commandBus)
        {
            _commandBus = commandBus;
        }

        public bool Handle(FlierModifiedEvent @event)
        {
            if (@event.NewState != null && @event.NewState.BrowserId == Guid.Empty.ToString())
            {
                _commandBus.Send(new MatchFlierToBoardsCommand() { FlierId = @event.NewState.Id });
            }
            return true;
        }
    }
}
