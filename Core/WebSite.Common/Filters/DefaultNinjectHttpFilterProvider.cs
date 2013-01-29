using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Ninject;
using Ninject.Syntax;

namespace Website.Common.Filters
{
    //this allows you to have properties with [Inject] in filter attiributes, haven't done one for mvc this is for http web api
    //NOTE attributes are only contruted once per controller, so if you need a service in transient
    //thread or request scope just inject resolution root and use as (crappy) service resolver.
    public class DefaultNinjectHttpFilterProvider : ActionDescriptorFilterProvider, IFilterProvider
    {
        private readonly IKernel _kernel;

        public DefaultNinjectHttpFilterProvider(IKernel kernel)
        {
            this._kernel = kernel;
        }

        public new IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            var filters = base.GetFilters(configuration, actionDescriptor).ToList();

            foreach (var filter in filters)
            {
                _kernel.Inject(filter.Instance);
            }

            return filters;
        }
    }
}
