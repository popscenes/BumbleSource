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
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public CreateImageCommandHandler(ContentStorageServiceInterface contentStorageService
            , GenericRepositoryInterface repository, UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _contentStorageService = contentStorageService;
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        #region Implementation of MessageHandlerInterface<in CreateImageCommand>

        public object Handle(CreateImageCommand command)
        {
            var insert = new Image()
                             {
                                 Id = command.MessageId,
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