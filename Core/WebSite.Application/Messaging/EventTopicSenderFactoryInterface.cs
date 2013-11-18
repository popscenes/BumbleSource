using System;

namespace Website.Application.Messaging
{
    public interface EventTopicSenderFactoryInterface
    {
        EventTopicSenderInterface GetTopicSender(Type type);
        EventTopicSenderInterface GetTopicSender(string name);
    }
}