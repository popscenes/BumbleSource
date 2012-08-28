using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Application.Command;

namespace Website.Application.Azure.Command
{
    public class AzureMessageFactory : MessageFactoryInterface
    {
        #region Implementation of MessageFactoryInterface

        public QueueMessageInterface GetMessageForBytes(byte[] bytes)
        {
            return new AzureCloudQueueMessage(bytes);
        }

        #endregion
    }
}
