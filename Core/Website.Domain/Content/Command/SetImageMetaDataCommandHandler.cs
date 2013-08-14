using System.Collections.Generic;
using Website.Domain.Content.Event;
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
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly ContentStorageServiceInterface _contentStorageService;
        private readonly EventPublishServiceInterface _publishService;
        private readonly GenericQueryServiceInterface _genericQueryService;


        public SetImageMetaDataCommandHandler(GenericRepositoryInterface repository
            , UnitOfWorkFactoryInterface unitOfWorkFactory
            , ContentStorageServiceInterface contentStorageService, EventPublishServiceInterface publishService, GenericQueryServiceInterface genericQueryService)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _contentStorageService = contentStorageService;
            _publishService = publishService;
            _genericQueryService = genericQueryService;
        }

        #region Implementation of MessageHandlerInterface<in SetImageStatusCommand>

        public object Handle(SetImageMetaDataCommand command)
        {
            var change = false;
            var oldstate = _genericQueryService.FindById<Image>(command.Id);
            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new List<RepositoryInterface>() {_repository}))
            {
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

                        });
            }

            if (unitOfWork.Successful && change)
            {
                _contentStorageService.SetMetaData(command);

                var newstate = _genericQueryService.FindById<Image>(command.Id);
                _publishService.Publish(new ImageModifiedEvent() { NewState = newstate, OrigState = oldstate });
            }
                

            return unitOfWork.Successful;
        }

        #endregion
    }
}