using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Syntax;

namespace WebSite.Infrastructure.Util
{
    public interface InitServiceInterface
    {
        void Init(IResolutionRoot iocContainer);
    }
}
