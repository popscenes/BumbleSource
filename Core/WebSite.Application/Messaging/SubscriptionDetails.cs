using System;

namespace Website.Application.Messaging
{
    public class SubscriptionDetails
    {
        public String Topic { get; set; }
        public String Subscription { get; set; }
        public Type HandlerType { get; set; }
    }
}