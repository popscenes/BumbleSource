using System;
using Website.Application.Binding;
using Website.Infrastructure.Command;
using Website.Application.Domain.Content.Command;
using Website.Domain.Content.Command;
using Website.Domain.Service;

namespace Website.Application.Domain.Content
{
    public class ImageProcessContentStorageService : ContentStorageServiceInterface
    {
        private readonly CommandBusInterface _commandBus;

        public ImageProcessContentStorageService([WorkerCommandBus]CommandBusInterface commandBus)
        {
            _commandBus = commandBus;
        }

        #region Implementation of ContentStorageServiceInterface

        public void Store(Website.Domain.Content.Content content, Guid id)
        {
            _commandBus.Send(new ImageProcessCommand()
                                 {
                                     ImageData = content.Data,
                                     CommandId = id.ToString()
                                 });
        }

        public void SetMetaData(SetImageMetaDataCommand initiatorCommand)
        {
            _commandBus.Send(new ImageProcessSetMetaDataCommand()
            {
                CommandId = initiatorCommand.CommandId,
                InitiatorCommand = initiatorCommand
            });
        }

        #endregion
    }
}