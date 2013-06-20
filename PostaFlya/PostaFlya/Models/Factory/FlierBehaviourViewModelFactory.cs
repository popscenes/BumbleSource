//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using Ninject;
//using PostaFlya.Areas.Default.Models;
//using PostaFlya.Domain.Behaviour;
//using PostaFlya.Domain.Flier;
//using PostaFlya.Domain.Service;
//using PostaFlya.Models.Flier;
//
//namespace PostaFlya.Models.Factory
//{
//    public class FlierBehaviourViewModelFactory : FlierBehaviourViewModelFactoryInterface,
//        FlierBehaviourViewModelFactoryRegistryInterface
//    {
//        private readonly BehaviourFactoryInterface _behaviourFactory;
//        private readonly Dictionary<Type, FlierBehaviourViewModelFactoryInterface> _factories = new Dictionary<Type, FlierBehaviourViewModelFactoryInterface>();
//
//        public FlierBehaviourViewModelFactory(BehaviourFactoryInterface behaviourFactory)
//        {
//            _behaviourFactory = behaviourFactory;
//        }
//
//        public BulletinFlierModel GetBulletinViewModel(FlierInterface flier, bool detailMode)
//        {
//            var type = _behaviourFactory.GetDefaultBehaviourTypeForBehaviour(flier.FlierBehaviour);
//
//            var fact = GetFactoryFor(type);
//
//            return fact.GetBulletinViewModel(flier, detailMode);
//        }
//
//        public DefaultDetailsViewModel GetBehaviourViewModel(FlierBehaviourInterface flierBehaviour)
//        {
//            var fact = GetFactoryFor(flierBehaviour.PrimaryInterface);
//            return fact.GetBehaviourViewModel(flierBehaviour);
//        }
//
//        private FlierBehaviourViewModelFactoryInterface GetFactoryFor(Type type)
//        {
//            return _factories[type];
//        }
//
//        public void RegisterViewModelFactory(Type type, FlierBehaviourViewModelFactoryInterface factory)
//        {
//            _factories[type] = factory;
//        }
//    }
//}