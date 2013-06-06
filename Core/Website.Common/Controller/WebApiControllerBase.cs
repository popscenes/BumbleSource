using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Website.Common.Filters;

namespace Website.Common.Controller
{
    [WebApiValidationActionFilter]
    public class WebApiControllerBase : ApiController 
    {
    }
}
