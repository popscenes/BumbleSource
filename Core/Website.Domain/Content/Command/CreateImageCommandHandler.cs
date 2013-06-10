using System;
using System.Collections.Generic;
using Website.Domain.Content.Event;
using Website.Infrastructure.Command;
using Website.Domain.Service;
using System.Linq;

namespace Website.Domain.Content.Command
{
    internal class CreateImageCommandHandler : CommandHandlerInterface<CreateImageCommand>
    {
        private readonly ContentStorageServiceInterface _contentStorageService;
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly DomainEventPublishServiceInterface _publishService;

        public CreateImageCommandHandler(ContentStorageServiceInterface contentStorageService
            , GenericRepositoryInterface repository, UnitOfWorkFactoryInterface unitOfWorkFactory, DomainEventPublishServiceInterface publishService)
        {
            _contentStorageService = contentStorageService;
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _publishService = publishService;
        }

        #region Implementation of CommandHandlerInterface<in CreateImageCommand>

        public object Handle(CreateImageCommand command)
        {
            var insert = new Image()
                             {
                                 Id = command.CommandId,
                                 Title = command.Title,
                                 BrowserId = command.Anonymous ? Guid.Empty.ToString() : command.BrowserId,
                                 Status = ImageStatus.Processing,
                                 Location = command.Location,
                                 ExternalId = command.ExternalId
                             };

            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork()))
            {
                _repository.Store(insert);
            }

            if (unitOfWork.Successful)
            {
                _contentStorageService.Store(command.Content, new Guid(insert.Id));
                _publishService.Publish(new ImageModifiedEvent() { NewState = insert });
            }
                
            return insert;
        }

        #endregion

        private IList<RepositoryInterface> GetReposForUnitOfWork()
        {
            return new List<RepositoryInterface>() { _repository };
        }
    }
}