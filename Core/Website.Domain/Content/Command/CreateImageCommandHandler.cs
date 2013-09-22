using System;
using System.Collections.Generic;
using Website.Infrastructure.Command;
using Website.Domain.Service;
using System.Linq;
using Website.Infrastructure.Messaging;

namespace Website.Domain.Content.Command
{
    internal class CreateImageCommandHandler : MessageHandlerInterface<CreateImageCommand>
    {
        private readonly ContentStorageServiceInterface _contentStorageService;
        private readonly GenericRepositoryInterface _repository;

        public CreateImageCommandHandler(ContentStorageServiceInterface contentStorageService
            , GenericRepositoryInterface repository)
        {
            _contentStorageService = contentStorageService;
            _repository = repository;
        }

        #region Implementation of MessageHandlerInterface<in CreateImageCommand>

        public void Handle(CreateImageCommand command)
        {
            var insert = new Image()
                             {
                                 Id = command.MessageId,
                                 Title = command.Title,
                                 BrowserId = command.Anonymous ? Guid.Empty.ToString() : command.BrowserId,
                                 Status = ImageStatus.Processing,
                                 Location = command.Location,
                                 ExternalId = command.ExternalId,
                                 Extension = command.KeepFileImapeType ? command.Content.Extension: "jpg"
                             };


            _repository.Store(insert);

            _contentStorageService.Store(command.Content, new Guid(insert.Id), command.KeepFileImapeType, command.Content.Extension);
                
        }

        #endregion

        private IList<RepositoryInterface> GetReposForUnitOfWork()
        {
            return new List<RepositoryInterface>() { _repository };
        }
    }
}