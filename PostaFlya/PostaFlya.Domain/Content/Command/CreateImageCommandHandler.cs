using System;
using System.Collections.Generic;
using PostaFlya.Domain.Service;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Domain.Content.Command
{
    internal class CreateImageCommandHandler : CommandHandlerInterface<CreateImageCommand>
    {
        private readonly ContentStorageServiceInterface _contentStorageService;
        private readonly ImageRepositoryInterface _imageRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public CreateImageCommandHandler(ContentStorageServiceInterface contentStorageService
                                         , ImageRepositoryInterface imageRepository, UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _contentStorageService = contentStorageService;
            _imageRepository = imageRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        #region Implementation of CommandHandlerInterface<in CreateImageCommand>

        public object Handle(CreateImageCommand command)
        {
            var insert = new Image()
                             {
                                 Id = command.CommandId,
                                 Title = command.Title,
                                 BrowserId = command.BrowserId,
                                 Status = ImageStatus.Processing,
                                 Location = command.Location
                             };
            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork()))
            {
                _imageRepository.Store(insert);
            }

            if (unitOfWork.Successful)
                _contentStorageService.Store(command.Content, new Guid(insert.Id));

            return insert;
        }

        #endregion

        private IList<RepositoryInterface> GetReposForUnitOfWork()
        {
            return new List<RepositoryInterface>() { _imageRepository };
        }
    }
}