using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Application.Domain.Browser;
using PostaFlya.Areas.WebApi.Boards.Model;
using PostaFlya.Domain.Boards.Query;
using Website.Application.Domain.Browser.Web;
using Website.Common.ApiInfrastructure.Controller;
using Website.Common.ApiInfrastructure.Model;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.WebApi.Boards.Controllers
{
    [BrowserAuthorizeHttp(Roles = "Participant")]
    public class WebBoardsByAdminController : WebApiControllerBase
    {
        private readonly PostaFlyaBrowserInformationInterface _browserInformation;
        private readonly QueryChannelInterface _queryChannel;

        public WebBoardsByAdminController(PostaFlyaBrowserInformationInterface browserInformation
            , QueryChannelInterface queryChannel)
        {
            _browserInformation = browserInformation;
            _queryChannel = queryChannel;
        }

        public ResponseContent<List<BoardSummaryModel>> Get()
        {
            var data =
                _queryChannel.Query(
                    new FindBoardsByAdminEmailQuery() {AdminEmail = _browserInformation.Browser.EmailAddress},
                    new List<BoardSummaryModel>());
            return ResponseContent<List<BoardSummaryModel>>.GetResponse(data);
        }
    }
}
