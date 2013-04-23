using System;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;

namespace Website.Application.Domain.Publish.Command
{
    [Serializable]
    public class DomainEventPublishCommand : DefaultCommandBase
    {
        public DomainEventInterface Event { get; set; }
    }
}