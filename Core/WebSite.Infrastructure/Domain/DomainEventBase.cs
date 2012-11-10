using System;

namespace Website.Infrastructure.Domain
{
    public class DomainEventBase : DomainEventInterface
    {
        public DomainEventBase()
        {
            TimeStamp = DateTimeOffset.UtcNow;
        }
        public DateTimeOffset TimeStamp { get; set; }
    }
}