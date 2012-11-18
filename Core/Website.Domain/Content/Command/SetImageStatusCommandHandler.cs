using System.Collections.Generic;
using Website.Infrastructure.Command;

namespace Website.Domain.Content.Command
{
    internal class SetImageStatusCommandHandler : CommandHandlerInterface<SetImageStatusCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SetImageStatusCommandHandler(GenericRepositoryInterface repository
                                            , UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        #region Implementation of CommandHandlerInterface<in SetImageStatusCommand>

        public object Handle(SetImageStatusCommand command)
        {
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