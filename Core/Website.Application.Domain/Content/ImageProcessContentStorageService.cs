using System;
using Website.Application.Binding;
using Website.Infrastructure.Binding;
using Website.Infrastructure.Command;
using Website.Application.Domain.Content.Command;
using Website.Domain.Content.Command;
using Website.Domain.Service;
using Website.Infrastructure.Messaging;

namespace Website.Application.Domain.Content
{
    public class ImageProcessContentStorageService : ContentStorageServiceInterface
    {
        private readonly MessageBusInterface _messageBus;

        public ImageProcessContentStorageService([WorkerCommandBus]MessageBusInterface messageBus)
        {
            _messageBus = messageBus;
        }

        #region Implementation of ContentStorageServiceInterface

        public void Store(Website.Domain.Content.Content content, Guid id)
        {
            Store(content, id, false, "jpg");
        }

        public void Store(Website.Domain.Content.Content content, Guid id, bool keepFileImapeType, string extension)
        {
            _messageBus.Send(new ImageProcessCommand()
            {
                ImageData = content.Data,
                MessageId = id.ToString(),
                KeepFileImapeType = keepFileImapeType,
                Extension = extension
            });
        }

        public void SetMetaData(SetImageMetaDataCommand initiatorCommand)
        {
            _messageBus.Send(new ImageProcessSetMetaDataCommand()
            {
                MessageId = initiatorCommand.MessageId,
                InitiatorCommand = initiatorCommand
            });
        }

        #endregion
    }
}