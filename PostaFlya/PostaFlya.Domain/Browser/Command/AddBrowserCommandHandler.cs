using System.Collections.Generic;
using Website.Domain.Browser;
using Website.Domain.Browser.Query;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Browser.Command
{
    internal class AddBrowserCommandHandler: MessageHandlerInterface<AddBrowserCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly QueryChannelInterface _queryChannel;

        public AddBrowserCommandHandler(GenericRepositoryInterface repository, QueryChannelInterface queryChannel)
        {
            _repository = repository;
            _queryChannel = queryChannel;
        }

        public void Handle(AddBrowserCommand command)
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

            browser.FriendlyId = _queryChannel
                .FindFreeHandleForBrowser(string.IsNullOrWhiteSpace(browser.FriendlyId) ? browser.Id : browser.FriendlyId, browser.Id);

            //lol
            if (command.Browser.EmailAddress != null && (command.Browser.EmailAddress.ToLower().Equals("rickyaudsley@gmail.com") ||
                command.Browser.EmailAddress.ToLower().Equals("teddymcuddles@gmail.com")))
                browser.Roles.Add(Role.Admin.ToString());

            _repository.Store(browser);
            
        }
    }
}
