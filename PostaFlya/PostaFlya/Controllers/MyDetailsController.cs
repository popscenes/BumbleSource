﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using PostaFlya.Models.Location;
using Website.Application.Domain.Browser.Web;
using Website.Common.Extension;
using Website.Infrastructure.Command;
using PostaFlya.Models.Browser;
using PostaFlya.Models.Tags;
using Website.Domain.Browser.Command;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.Controllers
{
    [BrowserAuthorize]
    public class MyDetailsController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly GenericQueryServiceInterface _queryService;

        public MyDetailsController(CommandBusInterface commandBus, GenericQueryServiceInterface queryService)
        {
            _commandBus = commandBus;
            _queryService = queryService;
        }

        public HttpResponseMessage Put(ProfileEditModel editModel)
        {
            var editProfileCommand = new ProfileEditCommand()
                                         {
                                             BrowserId = editModel.Id,                                         
                                             AddressPublic = editModel.AddressPublic,
                                             Handle = editModel.Handle,
                                             FirstName = editModel.FirstName,
                                             MiddleNames = editModel.MiddleNames,
                                             Surname = editModel.Surname,
                                             EmailAddress = editModel.Email,
                                             AvatarImageId = editModel.AvatarImageId
                                         };

            if (editModel.Address != null)
                editProfileCommand.Address = editModel.Address.ToDomainModel();

            var res = _commandBus.Send(editProfileCommand);
            return this.GetResponseForRes(res);            
        }

        public ProfileEditModel Get(string browserId)
        {
            var browser = _queryService.FindById<Website.Domain.Browser.Browser>(browserId);
            return new ProfileEditModel()
                       {
                           Id = browser.Id,
                           Address = browser.Address != null ? browser.Address.ToViewModel() : null,
                           AddressPublic = browser.AddressPublic,
                           Handle = browser.FriendlyId.EmptyIfNull(),
                           FirstName = browser.FirstName.EmptyIfNull(),
                           MiddleNames = browser.MiddleNames.EmptyIfNull(),
                           Surname = browser.Surname.EmptyIfNull(),
                           Email = browser.EmailAddress.EmptyIfNull(),
                           AvatarImageId = browser.AvatarImageId.EmptyIfNull(),
                           Credits = browser.AccountCredit
                       };
        }
    }
}
