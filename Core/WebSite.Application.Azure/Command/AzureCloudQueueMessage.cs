using Microsoft.WindowsAzure.Storage.Queue;
using Website.Application.Command;

namespace Website.Application.Azure.Command
{
    public class AzureCloudQueueMessage : QueueMessageInterface
    {
        private readonly CloudQueueMessage _queueMessage;
        public AzureCloudQueueMessage(byte[] content)
        {
            _queueMessage = new CloudQueueMessage(content);
        }

        public AzureCloudQueueMessage(string content) 
        {
            _queueMessage = new CloudQueueMessage(content);
        }

        public AzureCloudQueueMessage(CloudQueueMessage content)
        {
            _queueMessage = content;
        }

        #region Implementation of QueueMessageInterface

        public byte[] Bytes
        {
            get { return _queueMessage.AsBytes; }
            set { _queueMessage.SetMessageContent(value); }
        }

        #endregion

        internal CloudQueueMessage Message
        {
            get { return _queueMessage; }
        }

    }
}
