using System;
using System.Web.Http;
using System.Web.Http.Description;

namespace Website.Common.Obsolete
{
    [Obsolete("See ApiControllerBase in postaflya switching to that model")]
    [WebApiValidationActionFilter]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class OldWebApiControllerBase : ApiController 
    {
    }
}
