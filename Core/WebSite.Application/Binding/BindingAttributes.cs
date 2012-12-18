using System;

//These are used to identify different storage containers for injection
namespace Website.Application.Binding
{
    //blob storage contexts
    public class ImageStorageAttribute : Attribute { }
    //public class FlierImageStorageAttribute : Attribute { }

    //command bus contexts
    //worker command bus means that commands sent on the bus will go to workers 
    public class WorkerCommandBusAttribute : Attribute { }

    //for broadcast communication
    public class BroadcastCommunicatorAttribute : Attribute { }

    //for tiny url generation
    public class TinyUrlQueue : Attribute{} 
}
