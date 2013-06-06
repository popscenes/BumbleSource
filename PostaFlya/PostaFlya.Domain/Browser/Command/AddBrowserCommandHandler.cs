using System.Collections.Generic;
using Website.Domain.Browser.Query;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Browser.Command
{
    internal class AddBrowserCommandHandler: CommandHandlerInterface<AddBrowserCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericQueryServiceInterface _queryService;

        public AddBrowserCommandHandler(GenericRepositoryInterface repository, 
                                    UnitOfWorkFactoryInterface unitOfWorkFactory,
            GenericQueryServiceInterface queryService)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _queryService = queryService;
        }

        public object Handle(AddBrowserCommand command)
        {
            var browser = command.Browser;
            if(!string.IsNullOrWhiteSpace(browser.FriendlyId))
            {
                var parts = browser.FriendlyId.Split();
                int partsIndx = 0;

                if (parts.Length >= 1)
                    browser.FirstName = parts[partsIndx++];

                if (parts.Length > 2)
                    browser.MiddleNames = parts[partsIndx++];

                if (parts.Length >= 2)
                    browser.Surname = parts[partsIndx];
            }

            browser.FriendlyId = _queryService
                .FindFreeHandleForBrowser(string.IsNullOrWhiteSpace(browser.FriendlyId) ? browser.Id : browser.FriendlyId, browser.Id);

            var uow = _unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork());
            using (uow)
            {
                _repository.Store(browser);
            }

            if (uow.Successful)
                return new MsgResponse("Create Browser", false)
                    .AddCommandId(command)
                    .AddEntityId(command.Browser.Id);

            return new MsgResponse("Create Browser Failed", true)
                    .AddCommandId(command)
                    .AddEntityId(command.Browser.Id);
        }



        private IList<RepositoryInterface> GetReposForUnitOfWork()
        {
            return new List<RepositoryInterface>() { _repository };
        }
    }
}