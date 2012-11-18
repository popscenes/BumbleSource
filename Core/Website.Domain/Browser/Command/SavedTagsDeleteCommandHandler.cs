using System.Collections.Generic;
using System.Linq;
using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    internal class SavedTagsDeleteCommandHandler : CommandHandlerInterface<SavedTagsDeleteCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SavedTagsDeleteCommandHandler(GenericRepositoryInterface repository, 
                                      UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(SavedTagsDeleteCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork()))
            {
                _repository.UpdateEntity<Browser>(command.BrowserId
                    , browser =>
                          {
                              if (!browser.SavedTags.Any(t => t.Equals(command.Tags)))
                                  return;
                                browser.SavedTags.Remove(command.Tags);                              
                          });

                return new MsgResponse("Saved Tags Delete", false)
                    .AddMessageProperty("Tags", command.Tags.ToString())
                    .AddCommandId(command);
            }
        }

        private IList<RepositoryInterface> GetReposForUnitOfWork()
        {
            return new List<RepositoryInterface>() { _repository };
        }
    }
}