using System;
using System.Collections;
using System.Collections.Generic;
using Ninject;
using Ninject.Syntax;
using Website.Infrastructure.Query;

namespace Website.Infrastructure.Command
{
    public interface UnitOfWorkInterface : IDisposable
    {
        bool Successful { get; }
        UnitOfWorkInterface Begin();
        void End();
    }

    public interface UnitOfWorkForRepoInterface : UnitOfWorkInterface
    {
        GenericRepositoryInterface CurrentRepo { get;}
        GenericQueryServiceInterface CurrentQuery { get;}
    }

    public interface UnitOfWorkFactoryInterface
    {
        UnitOfWorkInterface GetUowInContext();
    }

    public interface UnitOfWorkForRepoFactoryInterface : UnitOfWorkFactoryInterface
    {
        new UnitOfWorkForRepoInterface GetUowInContext();
    }

    public class UnitOfWorkFactory : UnitOfWorkForRepoFactoryInterface
    {
        private readonly IResolutionRoot _resolutionRoot;

        public UnitOfWorkFactory(IResolutionRoot resolutionRoot)
        {
            _resolutionRoot = resolutionRoot;
        }

        public UnitOfWorkForRepoInterface GetUowInContext()
        {
            return _resolutionRoot.Get<UnitOfWorkForRepoInterface>();
        }

        UnitOfWorkInterface UnitOfWorkFactoryInterface.GetUowInContext()
        {
            return GetUowInContext();
        }
    }

    public class EmptyUnitOfWork : UnitOfWorkForRepoInterface
    {
        private readonly IResolutionRoot _resolutionRoot;

        public EmptyUnitOfWork(IResolutionRoot resolutionRoot = null)
        {
            _resolutionRoot = resolutionRoot;
        }

        public void Dispose()
        {
            End();
        }

        public bool Successful { get; private set; }
        public UnitOfWorkInterface Begin()
        {
            Successful = false;
            return this;
        }

        public void End()
        {
            Successful = true;
        }

        public GenericRepositoryInterface CurrentRepo
        {
            get
            {
                return _resolutionRoot == null
                           ? null
                           : _resolutionRoot.Get<GenericRepositoryInterface>();
            }
        }

        public GenericQueryServiceInterface CurrentQuery
        {
            get
            {
                return _resolutionRoot == null
                           ? null
                           : _resolutionRoot.Get<GenericQueryServiceInterface>();
            }
        }
    }
}