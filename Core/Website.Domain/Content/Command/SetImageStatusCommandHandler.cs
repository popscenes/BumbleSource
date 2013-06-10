using System.Collections.Generic;
using Website.Domain.Content.Event;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Query;

namespace Website.Domain.Content.Command
{
    internal class SetImageStatusCommandHandler : CommandHandlerInterface<SetImageStatusCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly DomainEventPublishServiceInterface _publishService;
        private readonly GenericQueryServiceInterface _genericQueryService;

        public SetImageStatusCommandHandler(GenericRepositoryInterface repository
                                            , UnitOfWorkFactoryInterface unitOfWorkFactory, DomainEventPublishServiceInterface publishService, GenericQueryServiceInterface genericQueryService)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _publishService = publishService;
            _genericQueryService = genericQueryService;
        }

        #region Implementation of CommandHandlerInterface<in SetImageStatusCommand>

        public object Handle(SetImageStatusCommand command)
        {
            var oldstate = _genericQueryService.FindById<Image>(command.Id);

            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new List<RepositoryInterface>() {_repository}))
            {
                _repository.UpdateEntity<Image>(command.Id, image => image.Status = command.Status);
            }

            if (unitOfWork.Successful)
            {
                var newstate = _genericQueryService.FindById<Image>(command.Id);
                _publishService.Publish(new ImageModifiedEvent() { NewState = newstate, OrigState = oldstate });
            }
            return unitOfWork.Successful;
        }

        #endregion
    }
}