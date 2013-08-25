using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;

namespace Website.Domain.Location.Command
{
    public class AddOrUpdateSuburbCommandHandler : MessageHandlerInterface<AddOrUpdateSuburbCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly QueryChannelInterface _queryChannel;

        public AddOrUpdateSuburbCommandHandler(GenericRepositoryInterface repository, UnitOfWorkFactoryInterface unitOfWorkFactory, QueryChannelInterface queryChannel)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _queryChannel = queryChannel;
        }

        public object Handle(AddOrUpdateSuburbCommand command)
        {
            //not wouldn't use this method in general, ie don't assume that something
            //doesn't exist because it can't be found by query.
            //however updating suburbs will only be an admin function and won't happen often
            var existing = _queryChannel.Query(new FindByIdQuery<Suburb>() { Id = command.Update.Id }, (Suburb)null);

            var uow = _unitOfWorkFactory.GetUnitOfWork(_repository);
            using (uow)
            {
                if (existing != null)
                {
                    _repository.UpdateEntity<Suburb>(command.Update.Id, suburb =>
                                                                        suburb.CopyFieldsFrom(command.Update));
                }
                else
                {
                    _repository.Store(command.Update);
                }
            }
            return true;
        }
    }
}