using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WebSite.Infrastructure.Command
{
    class UnitOfWorkFactory : UnitOfWorkFactoryInterface
    {
        #region Implementation of UnitOfWorkFactoryInterface

        public UnitOfWorkInterface GetUnitOfWork(IEnumerable contexts)
        {
            return new UnitOfWorkForRepositories(contexts);
        }

        #endregion
    }

    class UnitOfWorkForRepositories : UnitOfWorkInterface
    {
        private readonly IList<RepositoryInterface> _azureRepos;

        public UnitOfWorkForRepositories(RepositoryInterface repository)
        {
            _azureRepos = new List<RepositoryInterface>(){repository};
        }

        public UnitOfWorkForRepositories(IEnumerable repositories)
        {
            Successful = false;
            _azureRepos = repositories.OfType<RepositoryInterface>().ToList();
        }

        #region Implementation of UnitOfWorkInterface

        public void Dispose()
        {
            Successful = _azureRepos.All(a => a.SaveChanges());
        }

        public bool Successful { get; private set; }
        
        #endregion
    }
}
