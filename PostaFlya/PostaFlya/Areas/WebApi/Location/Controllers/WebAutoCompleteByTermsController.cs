using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PostaFlya.Areas.WebApi.Location.Model;
using Website.Common.ApiInfrastructure.Controller;
using Website.Common.ApiInfrastructure.Model;
using Website.Domain.Query;
using Website.Infrastructure.Query;

namespace PostaFlya.Areas.WebApi.Location.Controllers
{
    public class WebAutoCompleteByTermsController : WebApiControllerBase
    {
        private readonly QueryChannelInterface _queryChannel;

        public WebAutoCompleteByTermsController(QueryChannelInterface queryChannel)
        {
            _queryChannel = queryChannel;
        }

        public ResponseContent<List<AutoCompleteModel>> Get([FromUri] AutoCompleteRequest req)
        {
           var res = _queryChannel.Query(new AutoCompleteByTermsQuery()
                {
                    Terms = req.Q
                }, new List<AutoCompleteModel>());

           return ResponseContent<List<AutoCompleteModel>>.GetResponse(res);
        }
    }
}
