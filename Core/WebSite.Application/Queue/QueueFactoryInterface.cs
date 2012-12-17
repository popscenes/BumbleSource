using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Application.Queue
{
    public interface QueueFactoryInterface
    {
        QueueInterface GetQueue(string queueName);
        void DeleteQueue(string queueName);
    }
}
