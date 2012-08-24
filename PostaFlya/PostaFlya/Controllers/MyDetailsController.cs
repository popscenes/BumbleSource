using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using PostaFlya.Attributes;
using WebSite.Common.Extension;
using WebSite.Infrastructure.Command;
using PostaFlya.Models.Browser;
using PostaFlya.Models.Location;
using PostaFlya.Models.Tags;
using Website.Application.Domain.Browser;
using Website.Domain.Browser.Command;
using Website.Domain.Browser.Query;

namespace PostaFlya.Controllers
{
    [BrowserAuthorize]
    public class MyDetailsController : ApiController
    {
        private readonly CommandBusInterface _commandBus;
        private readonly BrowserInformationInterface _browserInformation;
        private readonly BrowserQueryServiceInterface _browserQueryService;
        
        public MyDetailsController(CommandBusInterface commandBus, BrowserQueryServiceInterface browserQueryService)
        {
            _commandBus = commandBus;
            _browserQueryService = browserQueryService;
        }

        public HttpResponseMessage Put(ProfileEditModel editModel)
        {
            var editProfileCommand = new ProfileEditCommand()
                                         {
                                             BrowserId = editModel.Id,
                                             Address = editModel.Address.ToDomainModel(),
                                             AddressPublic = editModel.AddressPublic,
                                             Handle = editModel.Handle,
                                             FirstName = editModel.FirstName,
                                             MiddleNames = editModel.MiddleNames,
                                             Surname = editModel.Surname,
                                             EmailAddress = editModel.Email,
                                             AvatarImageId = editModel.AvatarImageId
                                         };

            var res = _commandBus.Send(editProfileCommand);
            return this.GetResponseForRes(res);            
        }

        public ProfileEditModel Get(string browserId)
        {
            var browser = _browserQueryService.FindById<Website.Domain.Browser.Browser>(browserId);
            return new ProfileEditModel()
                       {
                           Id = browser.Id,
                           Address = browser.Address != null ? browser.Address.ToViewModel() : null,
                           AddressPublic = browser.AddressPublic,
                           Handle = browser.Handle.GetEmptyIfNull(),
                           FirstName = browser.FirstName.GetEmptyIfNull(),
                           MiddleNames = browser.MiddleNames.GetEmptyIfNull(),
                           Surname = browser.Surname.GetEmptyIfNull(),
                           Email = browser.EmailAddress.GetEmptyIfNull(),
                           AvatarImageId = browser.AvatarImageId.GetEmptyIfNull()
                       };
        }
    }
}
