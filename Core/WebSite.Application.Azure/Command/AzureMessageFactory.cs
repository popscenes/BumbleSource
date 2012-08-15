using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSite.Application.Command;

namespace WebSite.Application.Azure.Command
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
