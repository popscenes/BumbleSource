using System;
using System.Threading;

namespace WebSite.Application.Command
{
    public interface QueueInterface
    {
        void AddMessage(QueueMessageInterface message);
        QueueMessageInterface GetMessage();
        QueueMessageInterface GetMessage(TimeSpan invisibilityTimeOut);
        void DeleteMessage(QueueMessageInterface message);
    }
}