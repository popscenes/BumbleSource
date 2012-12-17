using System;
using Website.Application.Command;

namespace Website.Application.Queue
{
    public interface QueueInterface
    {
        void AddMessage(QueueMessageInterface message);
        QueueMessageInterface GetMessage();
        QueueMessageInterface GetMessage(TimeSpan invisibilityTimeOut);
        void DeleteMessage(QueueMessageInterface message);
    }
}