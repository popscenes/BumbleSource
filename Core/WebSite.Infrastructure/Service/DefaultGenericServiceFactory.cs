using System;
using Ninject;
using Ninject.Syntax;

namespace WebSite.Infrastructure.Service
{
    public class DefaultGenericServiceFactory: 
        GenericServiceFactoryInterface
    {
        private readonly IResolutionRoot _resolver;
        public DefaultGenericServiceFactory(IResolutionRoot resolver)
        {
            _resolver = resolver;
        }

        public AsType FindService<AsType>(Type serviceTyp, Type specialisedTyp) where AsType : class
        {
            var ret = _resolver.TryGet(serviceTyp.MakeGenericType(specialisedTyp));
            return ret as AsType;
        }
    }
}