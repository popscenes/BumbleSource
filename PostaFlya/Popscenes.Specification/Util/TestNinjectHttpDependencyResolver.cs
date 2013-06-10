using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Ninject;
using Ninject.Syntax;

namespace Popscenes.Specification.Util
{
    public class NinjectDependencyScope : IDependencyScope
    {
        private IResolutionRoot _resolver;
        private readonly HttpConfiguration _configuration;

        internal NinjectDependencyScope(IResolutionRoot resolver, HttpConfiguration configuration)
        {
            _resolver = resolver;
            _configuration = configuration;
        }

        public void Dispose()
        {
            _resolver = null;
        }

        private bool _resolvingDefaultService = false;
        public object GetService(Type serviceType)
        {
            if(_resolvingDefaultService)
                return null;
            
            object ret = null;
            if (_resolver == null)
                throw new ObjectDisposedException("this", "This scope has already been disposed");
            
            try
            {
                _resolvingDefaultService = true;
                ret = _configuration.Services.GetService(serviceType);
            }
            catch (Exception)//services throws exception when not a HTTP service, in which case use our container
            {
                ret = _resolver.TryGet(serviceType);//don't resolve default services
            }

            _resolvingDefaultService = false;

            return ret;
        }

        private bool _resolvingDefaultServices = false;
        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (_resolvingDefaultServices)
                return null;
            
            IEnumerable<object> ret = null;

            if (_resolver == null)
                throw new ObjectDisposedException("this", "This scope has already been disposed");

            try
            {
                _resolvingDefaultServices = true;
                ret = _configuration.Services.GetServices(serviceType);
            }
            catch (Exception)//services throws exception when not a HTTP service, in which case use our container
            {
                ret = _resolver.GetAll(serviceType);
            }
            
            _resolvingDefaultServices = false;
            return ret;
        }
    }
    /// <summary>
    /// This enables a mocking kernel or normal kernel to work in test scenarios. 
    /// It essentially tries to skip resolving http services and instead forces fall back to default service container. 
    /// </summary>
    public class TestNinjectHttpDependencyResolver : NinjectDependencyScope, IDependencyResolver
    {
        private readonly IKernel _kernel;
        private readonly HttpConfiguration _configuration;

        public TestNinjectHttpDependencyResolver(IKernel kernel, HttpConfiguration configuration)
            : base(kernel, configuration)
        {
            this._kernel = kernel;
            _configuration = configuration;
        }

        public IDependencyScope BeginScope()
        {
            return new NinjectDependencyScope(_kernel, _configuration);
        }
    }
}
