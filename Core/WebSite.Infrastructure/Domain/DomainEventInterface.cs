﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website.Infrastructure.Domain
{
    public interface DomainEventInterface
    {
        DateTimeOffset TimeStamp { get; set; }
    }
}
