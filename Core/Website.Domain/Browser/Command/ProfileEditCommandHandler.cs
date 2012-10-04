using System.Collections.Generic;
using Website.Infrastructure.Command;
using Website.Infrastructure.Domain;
using Website.Domain.Browser.Query;

namespace Website.Domain.Browser.Command
{
    internal class ProfileEditCommandHandler : CommandHandlerInterface<ProfileEditCommand>
    {
        private readonly BrowserRepositoryInterface _browserRepository;
        private readonly UnitOfWorkFactoryInterface _unitOfWorkFactory;
        private readonly BrowserQueryServiceInterface _browserQueryService;

        public ProfileEditCommandHandler(BrowserRepositoryInterface browserRepository
            , UnitOfWorkFactoryInterface unitOfWorkFactory
            , BrowserQueryServiceInterface browserQueryService)
        {
            _browserRepository = browserRepository;
            _unitOfWorkFactory = unitOfWorkFactory;
            _browserQueryService = browserQueryService;
        }

        public object Handle(ProfileEditCommand command)
        {
            var browser = _browserQueryService.FindById<Browser>(command.BrowserId);
            if (browser == null)
                return new MsgResponse("Error updating profile details", true)
                    .AddCommandId(command).AddEntityIdError(command.BrowserId);

            if(!string.IsNullOrWhiteSpace(command.Handle) && command.Handle != browser.Handle &&
                _browserQueryService.FindFreeHandle(command.Handle, browser.Id) != command.Handle.ToLower())
                    return new MsgResponse("Error updating profile details", true)
                        .AddCommandId(command).AddEntityIdError(command.BrowserId)
                        .AddMessageProperty("Handle", "Invalid Handle");


            UnitOfWorkInterface unitOfWork;
            using (unitOfWork = _unitOfWorkFactory.GetUnitOfWork(new List<object>() {_browserRepository}))
            {
                _browserRepository.UpdateEntity<Browser>(command.BrowserId
                    , browserUpdate =>
                        {
                            if(!string.IsNullOrWhiteSpace(command.Handle))
                                browserUpdate.Handle = command.Handle;
                            if (!string.IsNullOrWhiteSpace(command.FirstName))
                                browserUpdate.FirstName = command.FirstName;
                            if(!string.IsNullOrWhiteSpace(command.MiddleNames))
                                browserUpdate.MiddleNames = command.MiddleNames;
                            if (!string.IsNullOrWhiteSpace(command.Surname))
                                browserUpdate.Surname = command.Surname;
                            if (!string.IsNullOrWhiteSpace(command.EmailAddress))
                                browserUpdate.EmailAddress = command.EmailAddress;
                            if (command.Address.IsValid)
                                browserUpdate.Address = command.Address;
                            if (!string.IsNullOrWhiteSpace(command.AvatarImageId))
                                browserUpdate.AvatarImageId = command.AvatarImageId;
                            browserUpdate.AddressPublic = command.AddressPublic;
                        });    
            }
            var response = (!unitOfWork.Successful)
                                       ? new MsgResponse("Error updating profile details", true)
                                       : new MsgResponse("Profile details updated", false);

            return response.AddCommandId(command).AddEntityId(command.BrowserId);
        }
    }
}
