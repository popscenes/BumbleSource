using System.Collections.Generic;
using WebSite.Infrastructure.Command;

namespace Website.Domain.Content.Command
{
    internal class SetImageStatusCommandHandler : CommandHandlerInterface<SetImageStatusCommand>
    {
        private readonly ImageRepositoryInterface _imageRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SetImageStatusCommandHandler(ImageRepositoryInterface imageRepository
                                            , UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _imageRepository = imageRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        #region Implementation of CommandHandlerInterface<in SetImageStatusCommand>

        public object Handle(SetImageStatusCommand command)
        {
            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new List<RepositoryInterface>() {_imageRepository}))
            {
                _imageRepository.UpdateEntity<Image>(command.Id, image => image.Status = command.Status);
            }

            return unitOfWork.Successful;
        }

        #endregion
    }
}