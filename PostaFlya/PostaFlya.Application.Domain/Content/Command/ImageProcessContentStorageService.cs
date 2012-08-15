using System;
using WebSite.Application.Binding;
using PostaFlya.Domain.Content.Command;
using PostaFlya.Domain.Service;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Application.Domain.Content.Command
{
    public class ImageProcessContentStorageService : ContentStorageServiceInterface
    {
        private readonly CommandBusInterface _commandBus;

        public ImageProcessContentStorageService([WorkerCommandBus]CommandBusInterface commandBus)
        {
            _commandBus = commandBus;
        }

        #region Implementation of ContentStorageServiceInterface

        public void Store(PostaFlya.Domain.Content.Content content, Guid id)
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