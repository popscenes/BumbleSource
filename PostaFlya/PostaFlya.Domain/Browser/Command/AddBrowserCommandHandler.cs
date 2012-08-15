using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Browser.Query;
using WebSite.Infrastructure.Command;
using WebSite.Infrastructure.Domain;

namespace PostaFlya.Domain.Browser.Command
{
    internal class AddBrowserCommandHandler: CommandHandlerInterface<AddBrowserCommand>
    {
        private readonly BrowserRepositoryInterface _browserRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly BrowserQueryServiceInterface _browserQueryService;

        public AddBrowserCommandHandler(BrowserRepositoryInterface browserRepository, 
                                    UnitOfWorkFactoryInterface unitOfWorkFactory, 
            BrowserQueryServiceInterface browserQueryService)
        {
            _browserRepository = browserRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _browserQueryService = browserQueryService;
        }

        public object Handle(AddBrowserCommand command)
        {
            var browser = command.Browser;
            if(!string.IsNullOrWhiteSpace(browser.Handle))
            {
                var parts = browser.Handle.Split();
                int partsIndx = 0;

                if (parts.Length >= 1)
                    browser.FirstName = parts[partsIndx++];

                if (parts.Length > 2)
                    browser.MiddleNames = parts[partsIndx++];

                if (parts.Length >= 2)
                    browser.Surname = parts[partsIndx];
            }

            browser.Handle = BrowserQueryServiceUtil.FindFreeHandle( _browserQueryService,
                string.IsNullOrWhiteSpace(browser.Handle) ? browser.Id : browser.Handle, browser.Id);

            var uow = _unitOfWorkFactory.GetUnitOfWork(GetReposForUnitOfWork());
            using (uow)
            {
                _browserRepository.Store(browser);
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
            return new List<RepositoryInterface>() { _browserRepository };
        }
    }
}
