using System.Collections.Generic;
using Ninject.MockingKernel.Moq;
using Ninject.Modules;

namespace WebSite.Test.Common
{
    public class TestKernel
    {
        private readonly List<INinjectModule> _ninjectModules;
        public TestKernel(List<INinjectModule> ninjectModules)
        {
            _ninjectModules = ninjectModules;
        }

        private MoqMockingKernel _currIocKernel = null;
        public MoqMockingKernel Kernel
        {
            get
            {
                if (_currIocKernel == null)
                {
                    _currIocKernel = new MoqMockingKernel();
                    _currIocKernel.Load(NinjectModules);
                    _currIocKernel.Load(_ninjectModules);
                }
                return _currIocKernel;
            }
        }

        private static readonly List<INinjectModule> NinjectModules = new List<INinjectModule>()
                  {
                      
                  };
    }
}
