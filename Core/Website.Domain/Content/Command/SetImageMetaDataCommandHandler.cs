using System.Collections.Generic;
using Website.Infrastructure.Command;
using Website.Domain.Service;
using Website.Domain.Location;

namespace Website.Domain.Content.Command
{
    internal class SetImageMetaDataCommandHandler : CommandHandlerInterface<SetImageMetaDataCommand>
    {
        private readonly ImageRepositoryInterface _imageRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly ContentStorageServiceInterface _contentStorageService;

        public SetImageMetaDataCommandHandler(ImageRepositoryInterface imageRepository
            , UnitOfWorkFactoryInterface unitOfWorkFactory
            , ContentStorageServiceInterface contentStorageService)
        {
            _imageRepository = imageRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _contentStorageService = contentStorageService;
        }

        #region Implementation of CommandHandlerInterface<in SetImageStatusCommand>

        public object Handle(SetImageMetaDataCommand command)
        {
            var change = false;
            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new List<RepositoryInterface>() {_imageRepository}))
            {
                _imageRepository.UpdateEntity<Image>(command.Id,
                    image =>
                        {
                            if (command.Location != null && command.Location.IsValid())
                            {
                                image.Location = command.Location;
                                change = true;
                            }
                            if (!string.IsNullOrWhiteSpace(command.Title))
                            {
                                image.Title = command.Title;
                                change = true;
                            }
                        });
            }

            if (unitOfWork.Successful && change) 
                _contentStorageService.SetMetaData(command);

            return unitOfWork.Successful;
        }

        #endregion
    }
}