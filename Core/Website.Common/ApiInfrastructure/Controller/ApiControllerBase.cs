using System.Web.Http;
using System.Web.Http.Description;
using Website.Common.ApiInfrastructure.HttpFilters;

namespace Website.Common.ApiInfrastructure.Controller
{
    [ApiValidationActionFilter]
    public class ApiControllerBase : ApiController
    {
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public class WebApiControllerBase : ApiControllerBase
    {
        
    }
}