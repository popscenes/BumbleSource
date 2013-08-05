using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using PostaFlya.Domain.Browser.Command;
using PostaFlya.Models.Location;
using Website.Application.Domain.Browser.Web;
using Website.Application.Domain.Obsolete;
using Website.Common.Extension;
using Website.Common.Obsolete;
using Website.Infrastructure.Command;
using PostaFlya.Models.Browser;
using PostaFlya.Models.Tags;
using Website.Domain.Browser.Command;
using Website.Infrastructure.Query;
using Website.Infrastructure.Util.Extension;

namespace PostaFlya.Controllers
{
    [Website.Application.Domain.Obsolete.BrowserAuthorizeHttp]
    public class MyDetailsController : OldWebApiControllerBase
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
                                             Handle = editModel.Handle,
                                             FirstName = editModel.FirstName,
                                             MiddleNames = editModel.MiddleNames,
                                             Surname = editModel.Surname,
                                             EmailAddress = editModel.EmailAddress,
                                             AvatarImageId = editModel.AvatarImageId,
                                             WebSite = editModel.WebSite
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
                           Handle = browser.FriendlyId.EmptyIfNull(),
                           FirstName = browser.FirstName.EmptyIfNull(),
                           MiddleNames = browser.MiddleNames.EmptyIfNull(),
                           Surname = browser.Surname.EmptyIfNull(),
                           EmailAddress = browser.EmailAddress.EmptyIfNull(),
                           AvatarImageId = browser.AvatarImageId.EmptyIfNull(),
                           Credits = browser.AccountCredit,
                           WebSite = browser.WebSite
                       };
        }
    }
}
