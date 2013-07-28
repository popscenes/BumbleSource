using System;
using Microsoft.WindowsAzure.Storage.Queue;
using Website.Application.Azure.Content;
using Website.Application.Queue;

namespace Website.Application.Azure.Queue
{
    public class AzureCloudQueue : QueueInterface
    {
        private readonly CloudQueue _cloudQueue;

        private readonly TimeSpan _defaultInvisibilityTimeout = new TimeSpan(0,5,0);
        
        #region Implementation of QueueInterface

        public AzureCloudQueue(CloudQueue cloudQueue) 
        {
            _cloudQueue = cloudQueue;
        }

        public void AddMessage(QueueMessageInterface message)
        {
            var azureMsg = message as AzureCloudQueueMessage ?? new AzureCloudQueueMessage(message.Bytes);

            Func<bool> addMsg = () => { _cloudQueue.AddMessage(azureMsg.Message); return true;};
            AzureCloudBlobStorage.RetryQuery(addMsg);
                        
        }

        public QueueMessageInterface GetMessage(TimeSpan invisibilityTimeOut)
        {
            Func<QueueMessageInterface> getMessage = () =>
            {
                var message =
                    _cloudQueue.GetMessage(invisibilityTimeOut);
                return message != null
                        ? new AzureCloudQueueMessage(message)
                        : null;
            };
            return AzureCloudBlobStorage.RetryQuery(getMessage);
        }

        public QueueMessageInterface GetMessage()
        {
            return GetMessage(_defaultInvisibilityTimeout);
        }

        public void DeleteMessage(QueueMessageInterface message)
        {
            Func<bool> deleteMessage = () =>
            {
                var azureMsg = message as AzureCloudQueueMessage;
                if (azureMsg != null)
                    _cloudQueue.DeleteMessage(azureMsg.Message);
                return true;
            };
            AzureCloudBlobStorage.RetryQuery(deleteMessage);
        }

        public int? ApproximateMessageCount
        {
            get
            {
                _cloudQueue.FetchAttributes();
                return _cloudQueue.ApproximateMessageCount;
            }
        }

        #endregion

        public void Delete()
        {
            Func<bool> delete = () =>
            {
                _cloudQueue.Delete();
                return true;
            };
            AzureCloudBlobStorage.RetryQuery(delete);
        }

        public bool Exists()
        {
            Func<bool> exists = () => _cloudQueue.Exists();
            return AzureCloudBlobStorage.RetryQuery(exists);
        }

        public bool CreateIfNotExist()
        {
            Func<bool> create = () => _cloudQueue.CreateIfNotExists();
            return AzureCloudBlobStorage.RetryQuery(create);
        }
    }
}
