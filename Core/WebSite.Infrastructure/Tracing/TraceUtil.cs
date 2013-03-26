using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Infrastructure.Tracing
{
    public static class TraceUtil
    {
        public static string TraceError(Exception info)
        {
            Trace.TraceError("Unhandled Error: {0},\n Stack {1}", info.Message, info.StackTrace);
            return "error logged";
        }
    }
}
