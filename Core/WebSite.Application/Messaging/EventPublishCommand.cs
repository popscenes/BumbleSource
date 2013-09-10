using System;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace Website.Application.Messaging
{
    [Serializable]
    public class EventPublishCommand : DefaultCommandBase
    {
        public EventInterface Event { get; set; }
    }
}