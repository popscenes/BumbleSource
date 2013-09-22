using System.Collections.Generic;
using Website.Domain.Browser.Query;
using Website.Domain.Service;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Infrastructure.Messaging;
using Website.Infrastructure.Query;

namespace PostaFlya.Domain.Browser.Command
{
    internal class ProfileEditCommandHandler : MessageHandlerInterface<ProfileEditCommand>
    {
        private readonly GenericRepositoryInterface _repository;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly QueryChannelInterface _queryChannel;


        public ProfileEditCommandHandler(GenericRepositoryInterface repository
            , GenericQueryServiceInterface queryService, QueryChannelInterface queryChannel)
        {
            _repository = repository;
            _queryService = queryService;
            _queryChannel = queryChannel;
        }

        public void Handle(ProfileEditCommand command)
        {
            var browser = _queryService.FindById<Browser>(command.BrowserId);
            if (browser == null)
                return;

            if(!string.IsNullOrWhiteSpace(command.Handle) && command.Handle != browser.FriendlyId &&
                _queryChannel.FindFreeHandleForBrowser(command.Handle, browser.Id) != command.Handle.ToLower())
                    return;

            _repository.UpdateEntity<Browser>(command.BrowserId
                , browserUpdate =>
                    {
                        if(!string.IsNullOrWhiteSpace(command.Handle))
                            browserUpdate.FriendlyId = command.Handle;
                        if (!string.IsNullOrWhiteSpace(command.FirstName))
                            browserUpdate.FirstName = command.FirstName;
                        if(!string.IsNullOrWhiteSpace(command.MiddleNames))
                            browserUpdate.MiddleNames = command.MiddleNames;
                        if (!string.IsNullOrWhiteSpace(command.Surname))
                            browserUpdate.Surname = command.Surname;
                        if (!string.IsNullOrWhiteSpace(command.EmailAddress))
                            browserUpdate.EmailAddress = command.EmailAddress;
                        if (command.Address != null && command.Address.IsValid)
                            browserUpdate.Address = command.Address;
                        if (!string.IsNullOrWhiteSpace(command.AvatarImageId))
                            browserUpdate.AvatarImageId = command.AvatarImageId;
                        if (!string.IsNullOrWhiteSpace(command.WebSite))
                            browserUpdate.WebSite = command.WebSite;
                    });

        }
    }
}
