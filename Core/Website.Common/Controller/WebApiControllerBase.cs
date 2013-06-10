using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Website.Common.Filters;

namespace Website.Common.Controller
{
    [WebApiValidationActionFilter]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class WebApiControllerBase : ApiController 
    {
    }
}
