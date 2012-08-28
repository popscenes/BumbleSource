using System;
using System.Collections.Generic;
using WebSite.Infrastructure.Command;
using Website.Domain.Content.Query;
using Website.Domain.Service;
using System.Linq;

namespace Website.Domain.Content.Command
{
    internal class CreateImageCommandHandler : CommandHandlerInterface<CreateImageCommand>
    {
        private readonly ContentStorageServiceInterface _contentStorageService;
        private readonly ImageRepositoryInterface _imageRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly ImageQueryServiceInterface _imageQueryServiceInterface;

        public CreateImageCommandHandler(ContentStorageServiceInterface contentStorageService
                                         , ImageRepositoryInterface imageRepository, UnitOfWorkFactoryInterface unitOfWorkFactory, ImageQueryServiceInterface imageQueryServiceInterface)
        {
            _contentStorageService = contentStorageService;
            _imageRepository = imageRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _imageQueryServiceInterface = imageQueryServiceInterface;
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
                                 Location = command.Location,
                                 ExternalId = command.ExternalId
                             };

            if(!String.IsNullOrWhiteSpace(command.ExternalId))
            {
                var imagesList = _imageQueryServiceInterface.GetByBrowserId<Image>(command.BrowserId);
                var externalImage = imagesList.Where(_ => _.ExternalId == command.ExternalId);

                if(externalImage.Any())
                {
                    insert.Id = externalImage.First().Id;
                }

            }

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