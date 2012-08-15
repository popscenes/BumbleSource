using System;

namespace PostaFlya.Models.Factory
{
    public interface FlierBehaviourViewModelFactoryRegistryInterface
    {
        void RegisterViewModelFactory(Type type, FlierBehaviourViewModelFactoryInterface factory);
    }
}