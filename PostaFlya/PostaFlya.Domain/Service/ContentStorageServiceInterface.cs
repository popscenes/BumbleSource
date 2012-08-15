using System;
using PostaFlya.Domain.Content.Command;

namespace PostaFlya.Domain.Service
{
    public interface ContentStorageServiceInterface
    {
        void Store(Content.Content content, Guid id);
        void SetMetaData(SetImageMetaDataCommand initiatorCommand);
    }
}