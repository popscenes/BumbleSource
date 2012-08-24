using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostaFlya.Domain.Flier;
using WebSite.Infrastructure.Domain;
using WebSite.Infrastructure.Query;
//using WebSite.Infrastructure.Service;

namespace PostaFlya.Binding
{
//    public interface EntityQueryServiceFactoryInterface
//    {
//        ServiceType GetQueryServiceForEntityTyp<ServiceType>(EntityTypeEnum typeEnum) where ServiceType : class;
//    }
//
//    public class DefaultEntityQueryServiceFactory
//        : EntityQueryServiceFactoryInterface
//    {
//        private readonly GenericServiceFactoryInterface _genericServiceFactory;
//
//        public DefaultEntityQueryServiceFactory(GenericServiceFactoryInterface genericServiceFactory)
//        {
//            _genericServiceFactory = genericServiceFactory;
//        }
//
//        private readonly Type _serviceTyp = typeof(GenericQueryServiceInterface);
//        public ServiceType GetQueryServiceForEntityTyp<ServiceType>(EntityTypeEnum typeEnum) where ServiceType : class
//        {
//            switch (typeEnum)
//            {
//                case EntityTypeEnum.Flier:
//                    return _genericServiceFactory.FindService<ServiceType>(_serviceTyp, typeof (FlierInterface));
//                case EntityTypeEnum.Image:
//                    return _genericServiceFactory.FindService<ServiceType>(_serviceTyp, typeof(ImageInterface));
//                default:
//                    throw new ArgumentOutOfRangeException("typeEnum");
//            }
//            
//        }
//    }
}
