using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Website.Domain.TinyUrl;
using Website.Infrastructure.Configuration;

namespace Website.Application.Domain.TinyUrl.Web
{
    public class TinyUrlRouteConstraint : IRouteConstraint
    {

        private readonly TinyUrlServiceInterface _tinyUrlService;
        private readonly ConfigurationServiceInterface _configurationService;

        public TinyUrlRouteConstraint(TinyUrlServiceInterface tinyUrlService,
            ConfigurationServiceInterface configurationService )
        {
            _tinyUrlService = tinyUrlService;
            _configurationService = configurationService;
        }

        public bool Match(HttpContextBase httpContext, Route route,
            string parameterName, RouteValueDictionary values,
            RouteDirection routeDirection)
        {
            if (routeDirection == RouteDirection.IncomingRequest)
            {
                var ret =
                    _tinyUrlService.EntityInfoFor(httpContext.Request.Url.ToString());
                if (ret != null)
                {
                    httpContext.Items.Add("tinyUrlEntityInfo", ret);
                    values.Add("tinyUrlEntityInfo", ret);
                }
                return (ret != null);
            }
            return true;
        }
    }
}
