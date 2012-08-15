using System;
using System.Collections.Generic;
using System.Linq;
using WebSite.Infrastructure.Command;

namespace PostaFlya.Domain.Browser.Command
{
    internal class SavedTagsDeleteCommandHandler : CommandHandlerInterface<SavedTagsDeleteCommand>
    {
        private readonly BrowserRepositoryInterface _browserRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;

        public SavedTagsDeleteCommandHandler(BrowserRepositoryInterface browserRepository, 
                                      UnitOfWorkFactoryInterface unitOfWorkFactory)
        {
            _browserRepository = browserRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public object Handle(SavedTagsDeleteCommand command)
        {
            using (_unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork()))
            {
                _browserRepository.UpdateEntity(command.BrowserId
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
            return new List<RepositoryInterface>() { _browserRepository };
        }
    }
}