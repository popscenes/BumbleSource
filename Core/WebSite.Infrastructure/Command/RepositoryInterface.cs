using System;

namespace WebSite.Infrastructure.Command
{
    public static class RepositoryInterfaceExtensions
    {
        public static bool PerformActionInSingleUnitOfWork<RepositoryType>(this RepositoryType repository, Action<RepositoryType> action)
            where RepositoryType : RepositoryInterface
        {
            var uow = new UnitOfWorkForRepositories(repository);
            using (uow)
            {
                action(repository);
            }
            return uow.Successful;
        }
    }

    public interface RepositoryInterface
    {
        void Store(object entity);
        bool SaveChanges();
    }
}