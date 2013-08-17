using System.Collections.Generic;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;

namespace Website.Domain.Content.Command
{
    internal class SetImageStatusCommandHandler : MessageHandlerInterface<SetImageStatusCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericQueryServiceInterface _genericQueryService;

        public SetImageStatusCommandHandler(GenericRepositoryInterface repository
                                            , UnitOfWorkFactoryInterface unitOfWorkFactory, GenericQueryServiceInterface genericQueryService)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _genericQueryService = genericQueryService;
        }

        #region Implementation of MessageHandlerInterface<in SetImageStatusCommand>

        public object Handle(SetImageStatusCommand command)
        {
            var oldstate = _genericQueryService.FindById<Image>(command.Id);

            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new List<RepositoryInterface>() {_repository}))
            {
                _repository.UpdateEntity<Image>(command.Id, image => image.Status = command.Status);
            }

            return unitOfWork.Successful;
        }

        #endregion
    }
}