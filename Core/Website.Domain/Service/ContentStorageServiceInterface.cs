using System;
using Website.Domain.Content.Command;

namespace Website.Domain.Service
{
    public interface ContentStorageServiceInterface
    {
        void Store(Content.Content content, Guid id);
        void SetMetaData(SetImageMetaDataCommand initiatorCommand);
    }
}