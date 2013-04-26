using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Website.Infrastructure.Binding
{
    //for cache classes, defines the source data source rather than the cached one
    public class SourceDataSourceAttribute : Attribute { }
    //command bus contexts
    //worker command bus means that commands sent on the bus will go to workers 
    public class WorkerCommandBusAttribute : Attribute { }
}
