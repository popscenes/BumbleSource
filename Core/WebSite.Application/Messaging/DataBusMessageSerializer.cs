using Website.Application.Content;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Types;

namespace Website.Application.Messaging
{
    public class DataBusMessageSerializer : MessageSerializerInterface
    {
        private readonly BlobStorageInterface _blobStorage;

        //max 64k but base64 encoded just take off > 25% to ensure azure can handle for base 64 encoding;
        private const int MaxInlineMessageSize = 1024*8*5;

        public DataBusMessageSerializer(BlobStorageInterface blobStorage)
        {
            _blobStorage = blobStorage;
        }

        #region Implementation of CommandSerializerInterface

        public CommandType FromByteArray<CommandType>(byte[] array) where CommandType : class, MessageInterface
        {
            dynamic message = SerializeUtil.FromByteArray(array);
            if(message is DataBusRedirectToStorage)
            {
                byte[] data = _blobStorage.GetBlob(message.StorageId);
                message = SerializeUtil.FromByteArray(data);
            }

            if (message is JsonSerializedTypeContainer)
                return message.GetObject() as CommandType;
            
            return message as CommandType;
        }

        public byte[] ToByteArray<CommandType>(CommandType command) where CommandType : class, MessageInterface
        {
            //byte[] message = SerializeUtil.ToByteArray(JsonSerializedTypeContainer.Get(command));
            byte[] message = SerializeUtil.ToByteArray(command);
            if(message.Length >= MaxInlineMessageSize)
            {
                var redirect = new DataBusRedirectToStorage()
                                   {
                                       StorageId = command.MessageId
                                   };

                _blobStorage.SetBlob(redirect.StorageId, message);
                message = SerializeUtil.ToByteArray(redirect);
            }
            return message;
        }

        public void ReleaseCommand<CommandType>(CommandType command) where CommandType : class, MessageInterface
        {
            _blobStorage.DeleteBlob(command.MessageId);
        }

        #endregion
    }
}