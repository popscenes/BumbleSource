using System.Collections.Generic;
using Website.Infrastructure.Command;
using Website.Domain.Service;
using Website.Domain.Location;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;

namespace Website.Domain.Content.Command
{
    internal class SetImageMetaDataCommandHandler : MessageHandlerInterface<SetImageMetaDataCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly ContentStorageServiceInterface _contentStorageService;
        private readonly GenericQueryServiceInterface _genericQueryService;


        public SetImageMetaDataCommandHandler(GenericRepositoryInterface repository
            , ContentStorageServiceInterface contentStorageService, GenericQueryServiceInterface genericQueryService)
        {
            _repository = repository;
            _contentStorageService = contentStorageService;
            _genericQueryService = genericQueryService;
        }

        #region Implementation of MessageHandlerInterface<in SetImageStatusCommand>

        public void Handle(SetImageMetaDataCommand command)
        {
            var change = false;
            var oldstate = _genericQueryService.FindById<Image>(command.Id);
            UnitOfWorkInterface unitOfWork;

            _repository.UpdateEntity<Image>(command.Id,
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
                        if (command.Dimensions != null && command.Dimensions.Count > 0)
                        {
                            image.AvailableDimensions = command.Dimensions;
                        }

                        image.Extension = command.Extension;

                    });

            _contentStorageService.SetMetaData(command);

        }

        #endregion
    }
}