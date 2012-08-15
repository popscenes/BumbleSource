using WebSite.Application.Binding;
using WebSite.Application.Content;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Util;

namespace WebSite.Application.Command
{
    public class DataBusCommandSerializer : CommandSerializerInterface
    {
        private readonly BlobStorageInterface _blobStorage;

        //max 64k but base64 encoded just take off > 25% to ensure azure can handle;
        private const int MaxInlineMessageSize = 1024*8*5;

        public DataBusCommandSerializer(BlobStorageInterface blobStorage)
        {
            _blobStorage = blobStorage;
        }

        #region Implementation of CommandSerializerInterface

        public CommandType FromByteArray<CommandType>(byte[] array) where CommandType : class, CommandInterface
        {
            dynamic message = SerializeUtil.FromByteArray(array);
            if(message is DataBusRedirectToStorage)
            {
                byte[] data = _blobStorage.GetBlob(message.StorageId);
                message = SerializeUtil.FromByteArray(data);
            }
            
            return message as CommandType;
        }

        public byte[] ToByteArray<CommandType>(CommandType command) where CommandType : class, CommandInterface
        {
            byte[] message = SerializeUtil.ToByteArray(command);
            if(message.Length >= MaxInlineMessageSize)
            {
                var redirect = new DataBusRedirectToStorage()
                                   {
                                       StorageId = command.CommandId
                                   };

                _blobStorage.SetBlob(redirect.StorageId, message);
                message = SerializeUtil.ToByteArray(redirect);
            }
            return message;
        }

        public void ReleaseCommand<CommandType>(CommandType command) where CommandType : class, CommandInterface
        {
            _blobStorage.DeleteBlob(command.CommandId);
        }

        #endregion
    }
}