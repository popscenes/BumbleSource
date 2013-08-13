using System;

namespace Website.Infrastructure.Domain
{
    [Serializable]
    public class EventBase : EventInterface
    {
        public EventBase()
        {
            TimeStamp = DateTimeOffset.UtcNow;
        }
        public DateTimeOffset TimeStamp { get; set; }
    }
}