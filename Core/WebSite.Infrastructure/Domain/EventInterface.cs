using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Infrastructure.Messaging;

namespace Website.Infrastructure.Domain
{
    public interface EventInterface : MessageInterface
    {
        DateTimeOffset TimeStamp { get; set; }
    }
}
