using System;

//These are used to identify different storage containers for injection
namespace Website.Application.Binding
{
    //blob storage contexts
    public class ImageStorageAttribute : Attribute { }
    public class ApplicationStorageAttribute : Attribute { }

    //public class FlierImageStorageAttribute : Attribute { }



    //for broadcast communication
    public class BroadcastCommunicatorAttribute : Attribute { }

    //for queue
    public class QueueAttribute : Attribute {
        public string  Name { get; set; } 
    }

    public class ServiceTopicAttribute : Attribute
    {
        public string Name { get; set; }
    }

    public class ServiceTopicSubscrtipionAttribute : Attribute
    {
        public string TopicName { get; set; }
        public string SubscriptionName { get; set; }
    }



}
