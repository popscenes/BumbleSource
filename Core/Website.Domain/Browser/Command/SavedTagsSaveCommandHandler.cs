using System.Collections.Generic;
using System.Linq;
using Website.Infrastructure.Command;
using Website.Domain.Tag;

namespace Website.Domain.Browser.Command
{
    internal class SavedTagsSaveCommandHandler : CommandHandlerInterface<SavedTagsSaveCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SavedTagsSaveCommandHandler(GenericRepositoryInterface repository, 
                                      UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(SavedTagsSaveCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork()))
            {
                _repository.UpdateEntity<Browser>(command.BrowserId
                    , browser =>
                          {
                                if (browser.SavedTags.Any(t => t.Equals(command.Tags))) 
                                    return;
                                browser.SavedTags.Add(new Tags(command.Tags));                              
                          });
            }

            return new MsgResponse("Saved Tags Add", false)
                .AddMessageProperty("Tags", command.Tags.ToString())
                .AddCommandId(command);
        }

        private IList<RepositoryInterface> GetReposForUnitOfWork()
        {
            return new List<RepositoryInterface>() { _repository };
        }
    }
}