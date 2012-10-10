using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website.Application.Command
{
    public enum QueuedCommandResult
    {
        Successful,
        Error,
        Retry
    }
}
