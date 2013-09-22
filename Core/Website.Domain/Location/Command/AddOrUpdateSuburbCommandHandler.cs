using Website.Infrastructure.Command;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;

namespace Website.Domain.Location.Command
{
    public class AddOrUpdateSuburbCommandHandler : MessageHandlerInterface<AddOrUpdateSuburbCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly QueryChannelInterface _queryChannel;

        public AddOrUpdateSuburbCommandHandler(GenericRepositoryInterface repository, QueryChannelInterface queryChannel)
        {
            _repository = repository;
            _queryChannel = queryChannel;
        }

        public void Handle(AddOrUpdateSuburbCommand command)
        {
            //not wouldn't use this method in general, ie don't assume that something
            //doesn't exist because it can't be found by query.
            //however updating suburbs will only be an admin function and won't happen often
            var existing = _queryChannel.Query(new FindByIdQuery<Suburb>() { Id = command.Update.Id }, (Suburb)null);

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
    }
}