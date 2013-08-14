using System;

namespace Website.Application.Queue
{
    public interface QueueInterface : QueueSenderInterface, QueueReceiverInterface
    {
        int? ApproximateMessageCount { get; }
    }

    public interface QueueReceiverInterface
    {
        QueueMessageInterface GetMessage();
        QueueMessageInterface GetMessage(TimeSpan invisibilityTimeOut);
        void DeleteMessage(QueueMessageInterface message);
    }

    public interface QueueSenderInterface
    {
        void AddMessage(QueueMessageInterface message);
    }
}