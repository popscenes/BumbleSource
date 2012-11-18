using System.Collections.Generic;
using System.Linq;
using Website.Infrastructure.Command;

namespace Website.Domain.Browser.Command
{
    internal class SavedTagsSelectCommandHandler : CommandHandlerInterface<SavedTagsSelectCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SavedTagsSelectCommandHandler(GenericRepositoryInterface repository, 
                                      UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(SavedTagsSelectCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork()))
            {
                _repository.UpdateEntity<Browser>(command.BrowserId
                    , browser =>
                          {
                            if (browser.Tags.Equals(command.Tags) ||
                            !browser.SavedTags.Any(t => t.Equals(command.Tags)))
                                return;
                            browser.Tags = command.Tags;                              
                          });
            }

            return new MsgResponse("Saved Tags Select", false)
                .AddMessageProperty("Tags", command.Tags.ToString())
                .AddCommandId(command);
        }

        private IList<RepositoryInterface> GetReposForUnitOfWork()
        {
            return new List<RepositoryInterface>() { _repository };
        }
    }
}