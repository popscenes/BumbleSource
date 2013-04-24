using System;

namespace Website.Infrastructure.Domain
{
    [Serializable]
    public class DomainEventBase : DomainEventInterface
    {
        public DomainEventBase()
        {
            TimeStamp = DateTimeOffset.UtcNow;
        }
        public DateTimeOffset TimeStamp { get; set; }
    }
}