using System.Collections.Generic;
using System.Linq;
using PostaFlya.Domain.Tag;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Domain.Browser.Command
{
    internal class SavedTagsSaveCommandHandler : CommandHandlerInterface<SavedTagsSaveCommand>
    {
        private readonly BrowserRepositoryInterface _browserRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SavedTagsSaveCommandHandler(BrowserRepositoryInterface browserRepository, 
                                      UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _browserRepository = browserRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(SavedTagsSaveCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork()))
            {
                _browserRepository.UpdateEntity(command.BrowserId
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
            return new List<RepositoryInterface>() { _browserRepository };
        }
    }
}