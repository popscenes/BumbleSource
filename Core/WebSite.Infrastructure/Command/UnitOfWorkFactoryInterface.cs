using System;
using System.Collections;
using System.Collections.Generic;

namespace Website.Infrastructure.Command
{
    public interface UnitOfWorkFactoryInterface
    {
        UnitOfWorkInterface GetUnitOfWork(IEnumerable contexts);
    }

    public interface UnitOfWorkInterface : IDisposable
    {
        bool Successful { get; }
    }
}