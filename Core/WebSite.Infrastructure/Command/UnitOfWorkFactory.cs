using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Website.Infrastructure.Command
{
    class UnitOfWorkFactory : UnitOfWorkFactoryInterface
    {
        #region Implementation of UnitOfWorkFactoryInterface

        public UnitOfWorkInterface GetUnitOfWork(IEnumerable contexts)
        {
            return new UnitOfWorkForRepositories(contexts);
        }

        public UnitOfWorkInterface GetUnitOfWork(params object[] contexts)
        {
            return new UnitOfWorkForRepositories(contexts);
        }

        #endregion
    }

    class UnitOfWorkForRepositories : UnitOfWorkInterface
    {
        private readonly IList<RepositoryInterface> _repos;

        public UnitOfWorkForRepositories(RepositoryInterface repository)
        {
            _repos = new List<RepositoryInterface>(){repository};
        }

        public UnitOfWorkForRepositories(IEnumerable repositories)
        {
            Successful = false;
            _repos = repositories.OfType<RepositoryInterface>().ToList();
        }

        #region Implementation of UnitOfWorkInterface

        public void Dispose()
        {
            Successful = _repos.All(a => a.SaveChanges());
        }

        public bool Successful { get; private set; }
        
        #endregion
    }
}
