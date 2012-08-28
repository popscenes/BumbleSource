using System;
using System.Threading;

namespace Website.Application.Command
{
    public interface QueueInterface
    {
        void AddMessage(QueueMessageInterface message);
        QueueMessageInterface GetMessage();
        QueueMessageInterface GetMessage(TimeSpan invisibilityTimeOut);
        void DeleteMessage(QueueMessageInterface message);
    }
}