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
        private readonly GenericQueryServiceInterface _genericQueryService;

        public SetImageStatusCommandHandler(GenericRepositoryInterface repository, GenericQueryServiceInterface genericQueryService)
        {
            _repository = repository;
            _genericQueryService = genericQueryService;
        }

        #region Implementation of MessageHandlerInterface<in SetImageStatusCommand>

        public void Handle(SetImageStatusCommand command)
        {
            _genericQueryService.FindById<Image>(command.Id);

            _repository.UpdateEntity<Image>(command.Id, image => image.Status = command.Status);

        }

        #endregion
    }
}