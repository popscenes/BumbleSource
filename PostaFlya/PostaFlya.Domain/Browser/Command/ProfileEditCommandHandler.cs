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
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly GenericQueryServiceInterface _queryService;
        private readonly QueryChannelInterface _queryChannel;


        public ProfileEditCommandHandler(GenericRepositoryInterface repository
            , UnitOfWorkFactoryInterface unitOfWorkFactory
            , GenericQueryServiceInterface queryService, QueryChannelInterface queryChannel)
        {
            _repository = repository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _queryService = queryService;
            _queryChannel = queryChannel;
        }

        public object Handle(ProfileEditCommand command)
        {
            var browser = _queryService.FindById<Browser>(command.BrowserId);
            if (browser == null)
                return new MsgResponse("Error updating profile details", true)
                    .AddCommandId(command).AddEntityIdError(command.BrowserId);

            if(!string.IsNullOrWhiteSpace(command.Handle) && command.Handle != browser.FriendlyId &&
                _queryChannel.FindFreeHandleForBrowser(command.Handle, browser.Id) != command.Handle.ToLower())
                    return new MsgResponse("Error updating profile details", true)
                        .AddCommandId(command).AddEntityIdError(command.BrowserId)
                        .AddMessageProperty("Handle", "Invalid Handle");


            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new List<object>() {_repository}))
            {
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
            var response = (!unitOfWork.Successful)
                                       ? new MsgResponse("Error updating profile details", true)
                                       : new MsgResponse("Profile details updated", false);


            return response.AddCommandId(command).AddEntityId(command.BrowserId);
        }
    }
}
